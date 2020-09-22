using System;
using PressureSensorTestCore;
using ArchvingTestResult;

namespace PressureSensorTest
{
    public class SavingResults
    {
        readonly Settings settings;
        readonly ArchivingProcess archiving;
        readonly ProductDb db;
        readonly SystemStatus sysStatus;

        public SavingResults(Settings settings, SystemStatus sysStatus)
        {
            this.settings = settings;
            this.sysStatus = sysStatus;
            var jsonSettings = settings.JsonReportSettings;
            archiving = new ArchivingProcess(jsonSettings.ArchivingPath, jsonSettings.MaxCommunicationBreakWithArchive, 
                jsonSettings.UsedFtp);
            archiving.SuccessfulCopyToServerEvent += Archiving_SuccessfulCopyToServerEvent;
            archiving.StartTracking();
            db = new ProductDb(settings.PathToDb);
        }

        private void Archiving_SuccessfulCopyToServerEvent(object sender, EventArgs e)
        {
            sysStatus.ServerStatus = StatusEnum.Ok;
        }

        public void CheckSavingState()
        {
            archiving.CheckLocalFolderState();
        }

        public void SaveResult(ProductInfo product, TestResults results, IDialogService dialogService)
        {
            // Смысл формировать и посылать Json есть только когда статус продукта 0 - нет ошибок, или 14 - неудачная поверка, или 18 - забракован оператором
            // Если были другие ошибки, то и результатов нет, формировать Json не из чего
            product.ClosingDateTime = DateTime.Now;
            try
            {
                string jsonContent = null;
                string fileName = null;

                if ((product.Error == TestErrorEnum.NoError || product.Error == TestErrorEnum.BadPrecision ||
                    product.Error == TestErrorEnum.OperatorSolution))
                {
                    var jsonAdapter = new JsonAdapter(settings.JsonReportSettings);
                    jsonAdapter.AddResults(product.Device.SerialNumber, product.Device.DeviceTypeCode, product.Device.SensorTypeCode, product.PrimaryTest, 
                        results, new PressureIndication(product.Device.Range.Pressure_Pa), product.Error == 0, product.ClosingDateTime);
                    jsonContent = jsonAdapter.JsonReportResult.GetJsonByString();
                    fileName = jsonAdapter.JsonReportResult.GetFileName();
                }

                // Передача в базу стенда
                if (settings.UsedStandDatabase)
                {
                    db.AddTestInfo(product, jsonContent);
                    sysStatus.DataBaseStatus = StatusEnum.Ok;
                }

                // Передача на сервер
                if (settings.JsonReportSettings.ArchivingJsonFile && !string.IsNullOrEmpty(fileName))
                {
                    archiving.Save(fileName, jsonContent);
                    sysStatus.ServerStatus = StatusEnum.Ok;
                }
            }
            catch(DbException ex)
            {
                sysStatus.DataBaseStatusMessage = ex.Message;
                sysStatus.DataBaseStatus = StatusEnum.Error;

                if (!settings.UsedRemoteControl)
                {
                    bool dialResult = dialogService.ErrorTwoButtonDialog(ex.Message + "\r\nЧтобы повторить попытку нажмите \"OK\". Для отмены операции нажмите \"Отмена\"");
                    if (dialResult)
                        SaveResult(product, results, dialogService); // Повторим операцию
                    else
                        throw new OperationCanceledException();
                }
                else
                {
                    throw new OperationCanceledException();
                }
            }
            catch(SavingToLocalFolderException ex)
            {
                string message = "Не удалось сохранить результаты в формате JSON ни на удаленном сервере, ни в локальной директории:\r\n"
                    + ex.Message;
                sysStatus.ServerStatusMessage = message;
                sysStatus.ServerStatus = StatusEnum.Error;
                dialogService.ErrorMessage(message);
                throw new OperationCanceledException();
            }
            catch(SavingToRemoteArchiveException ex)
            {
                // Json сохранился в локальной папке, можно продолжать. При восстановлении связи, файлы будут переданы на сервыер
                sysStatus.ServerStatusMessage = ex.Message;
                sysStatus.ServerStatus = StatusEnum.Warning;
                dialogService.WarningMessage(ex.Message);
            }
        }
    }
}
