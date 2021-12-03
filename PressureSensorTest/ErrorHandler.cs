using System;
using PressSystems;
using SDM_comm;
using PressureSensorTestCore;

namespace PressureSensorTest
{
    public class ErrorHandler
    {
        protected Settings settings;
        protected SystemStatus sysStatus;
        protected IDialogService dialogService;

        public ErrorHandler(Settings settings, SystemStatus sysStatus, IDialogService dialogService)
        {
            this.settings = settings;
            this.sysStatus = sysStatus;
            this.dialogService = dialogService;
        }

        public virtual void ErrorHanding(Exception exception, ProductInfo product)
        {
            try
            {
                throw exception;
            }
            catch(OperationCanceledException)
            { }
            // Утечка в системе
            catch (SetPressureTimeoutException ex)
            {
                product.Error = TestErrorEnum.Leakage;
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
                product.Error = TestErrorEnum.RangeNotSupportByPsys;
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
            catch(SDM_ErrException ex)
            {
                // Выводим сообщение и отменяем операцию
                product.Error = TestErrorEnum.SystemError;
                sysStatus.AmmetrStatusMessage = ex.Message;
                sysStatus.AmmetrStatus = StatusEnum.Error;               
                Cancel(ex.Message);
            }

            // Авария пневмосистемы
            catch (PressSystemException ex)
            {
                // Выводим сообщение и отменяем операцию
                product.Error = TestErrorEnum.SystemError;
                sysStatus.PressSystemStatusMessage = ex.Message;
                sysStatus.PressSystemStatus = StatusEnum.Error;
                Cancel(ex.Message);
            }
            // Неисправность изделия. Измеренный ток ниже допустимого предела
            catch (LoCurrentAlarmException)
            {
                product.Error = TestErrorEnum.AlarmLoLimit;
                Cancel("Измеренный ток ниже допустимого предела");
            }
            // Неисправность изделия. Измеренный ток выше допустимого предела
            catch (HiCurrentAlarmException)
            {
                product.Error = TestErrorEnum.AlarmHiLimit;
                Cancel("Измеренный ток выше допустимого предела");
            }
            // Ошибка открытия порта цифрового датчика
            catch (OpenConnectPortException)
            {
                product.Error = TestErrorEnum.OpenPortError;
                Cancel("Ошибка открытия порта связи с поверяемым ПД");
            }
            // Ошибка подключения (для датчиков с цифровым интерфейсом)
            catch (ConnectException)
            {
                product.Error = TestErrorEnum.ConnectError;
                Cancel("Не удалось установить связь с поверяемым ПД");
            }
            // Потеря связи (для датчиков с цифровым интерфейсом)
            catch (LostConnectException)
            {
                product.Error = TestErrorEnum.LostConnect;
                Cancel("Потеря связи связи с поверяемым ПД");
            }
            // Непредусмотренные ошибки
            catch (Exception ex)
            {
                product.Error = TestErrorEnum.SystemError;
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
            dialogService?.ErrorMessage(message);
            throw new OperationCanceledException();
        }
    }

    
}
