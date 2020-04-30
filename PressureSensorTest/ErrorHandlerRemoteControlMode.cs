using System;
using PressSystems;
using SDM_comm;

namespace PressureSensorTest
{
    public class ErrorHandlerRemoteControlMode: ErrorHandler
    {
        public ErrorHandlerRemoteControlMode(Settings settings, SystemStatus sysStatus):
            base(settings, sysStatus)
        { }

        public override void ErrorHanding(Exception exception, ProductInfo product, IDialogService dialogService)
        {
            base.dialogService = dialogService;
            try
            {
                throw exception;
            }
            catch (OperationCanceledException)
            { }
            // Утечка в системе
            catch (SetPressureTimeoutException)
            {
                product.Error = ProcessErrorEnum.Leakage;
                throw new OperationCanceledException();
            }

            // Изделие не поддерживается пневмосистемой
            catch (PsysPrecissionException)
            {
                product.Error = ProcessErrorEnum.RangeNotSupportByPsys;
                throw new OperationCanceledException();
            }

            // Авария амперметра
            catch (AmmetrErrException ex)
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
            catch (Exception ex)
            {
                product.Error = ProcessErrorEnum.SystemError;
                Cancel("Непредусмотренная ошибка. Обратитесь к разработчику:\r\n" + ex.Message);
            }
        }
    }
}
