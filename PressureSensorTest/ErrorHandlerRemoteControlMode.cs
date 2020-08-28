using System;
using PressSystems;
using SDM_comm;
using OwenPressureDevices;


namespace PressureSensorTest
{
    public class ErrorHandlerRemoteControlMode: ErrorHandler
    {
        public ErrorHandlerRemoteControlMode(Settings settings, SystemStatus sysStatus):
            base(settings, sysStatus)
        {
        }

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
                product.Error = TestErrorEnum.Leakage;
                throw new OperationCanceledException();
            }

            // Изделие не поддерживается пневмосистемой
            catch (PsysSupportException)
            {
                product.Error = TestErrorEnum.RangeNotSupportByPsys;
                throw new OperationCanceledException();
            }

            // Авария амперметра
            catch (SDM_ErrException ex)
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

            catch (ParseDeviceNameException)
            {
                // Выводим сообщение и отменяем операцию
                throw;
            }

            // Непредусмотренные ошибки
            catch (Exception ex)
            {
                product.Error = TestErrorEnum.SystemError;
                Cancel("Непредусмотренная ошибка. Обратитесь к разработчику:\r\n" + ex.Message);
            }
        }
    }
}
