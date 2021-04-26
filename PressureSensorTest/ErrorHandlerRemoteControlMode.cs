using System;
using PressSystems;
using SDM_comm;
using OwenPressureDevices;
using PressureSensorTestCore;


namespace PressureSensorTest
{
    public class ErrorHandlerRemoteControlMode: ErrorHandler
    {
        public ErrorHandlerRemoteControlMode(Settings settings, SystemStatus sysStatus):
            base(settings, sysStatus, null)
        {
        }

        public override void ErrorHanding(Exception exception, ProductInfo product)
        {
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

            // Неисправность изделия. Измеренный ток ниже нижнего предела
            catch (LoCurrentAlarmException)
            {
                product.Error = TestErrorEnum.AlarmLoLimit;
                throw new OperationCanceledException();
            }
            // Неисправность изделия. Измеренный ток выше верхнего предела
            catch (HiCurrentAlarmException)
            {
                product.Error = TestErrorEnum.AlarmHiLimit;
                throw new OperationCanceledException();
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
