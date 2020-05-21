using System;
using PressSystems;
using SDM_comm;

namespace PressureSensorTest
{
    public class ErrorHandler
    {
        protected Settings settings;
        protected SystemStatus sysStatus;
        protected IDialogService dialogService;

        public ErrorHandler(Settings settings, SystemStatus sysStatus)
        {
            this.settings = settings;
            this.sysStatus = sysStatus;
        }

        public virtual void ErrorHanding(Exception exception, ProductInfo product, IDialogService dialogService)
        {
            this.dialogService = dialogService;
            try
            {
                throw exception;
            }
            catch(OperationCanceledException)
            { }
            // Утечка в системе
            catch (SetPressureTimeoutException ex)
            {
                product.Error = ProcessErrorEnum.Leakage;
                string message = ex.Message + ". Возможно, нарушена герметичность изделия";
                if (!settings.UsedStandDatabase)
                {
                    Cancel(message);
                }
                else
                {
                    // Работа с базой
                    // Спрашиваем, браковать или нет
                    RejectDialog(message, (int)product.Error);
                }
            }

            // Изделие не поддерживается пневмосистемой
            catch(PsysSupportException ex)
            {
                product.Error = ProcessErrorEnum.RangeNotSupportByPsys;
                if (!settings.UsedStandDatabase)
                {
                    Cancel(ex.Message);
                }
                else
                {
                    // Работа с базой
                    // Спрашиваем, браковать или нет
                    RejectDialog(ex.Message, (int)product.Error);                  
                }
            }

            // Авария амперметра
            catch(AmmetrErrException ex)
            {
                // Выводим сообщение и отменяем операцию
                product.Error = ProcessErrorEnum.SystemError;
                sysStatus.AmmetrStatusMessage = ex.Message;
                sysStatus.AmmetrStatus = StatusEnum.Error;               
                Cancel(ex.Message);
            }

            // Авария пневмосистемы
            catch (PressSystemException ex)
            {
                // Выводим сообщение и отменяем операцию
                product.Error = ProcessErrorEnum.SystemError;
                sysStatus.PressSystemStatusMessage = ex.Message;
                sysStatus.PressSystemStatus = StatusEnum.Error;
                Cancel(ex.Message);
            }
            // Непредусмотренные ошибки
            catch(Exception ex)
            {
                product.Error = ProcessErrorEnum.SystemError;
                Cancel("Непредусмотренная ошибка. Обратитесь к разработчику:\r\n" + ex.Message);
            }
        }

        protected void RejectDialog(string message, int error)
        {
            string mess = string.Format("{0}. Для забраковки изделия с ошибкой {1} нажмите \"ОК\", для отмены операции нажмите \"Отмена\"",
                message, error);
            if (!dialogService.ErrorTwoButtonDialog(mess))
                throw new OperationCanceledException(); // Отмена операции
        }

        protected void Cancel(string message)
        {
            dialogService.ErrorMessage(message);
            throw new OperationCanceledException();
        }
    }

    
}
