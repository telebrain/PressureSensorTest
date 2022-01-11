using System;


namespace ProductDatabase2
{
    public class ConnectToBuffersException: Exception
    {
        public override string Message { get; }
        public ConnectToBuffersException(Exception exception,  string dbName)
        {
            Message = $"Не удалось подключиться к базе данных {dbName}: " + exception.Message;
        }
    }

    [Serializable]
    public class ConnectToDbException : Exception
    {
        public override string Message { get; }
        public ConnectToDbException(Exception exception, string dbName)
        {
            Message = $"Не удалось подключиться к базе данных {dbName}: " + exception.Message;
        }
    }

    [Serializable]
    public class InputBufferException: Exception { }

    [Serializable]
    public class WriteToInputBufferException : InputBufferException
    {
        public override string Message { get; } 
        public WriteToInputBufferException(Exception ex, string dbName, string tableName, string sn)
        {
            Message = $"Не удалось добавить информацию об изделии с заводским номером {sn} в таблицу {tableName} базы {dbName}: {ex.Message}";
        }
    }

    [Serializable]
    public class AddToInputBufferException : InputBufferException
    {
        public override string Message { get; }
        public AddToInputBufferException(string dbName, string tableName)
        {
            Message = $"Информация об изделии не может быть добавлена в таблицу {tableName} базы {dbName}, так как уже есть одна запись. Буфер не может содержать более одной записи";
        }
    }

    [Serializable]
    public class InputBufferStateException : InputBufferException
    {
        public override string Message { get; }
        public InputBufferStateException(string dbName, string tableName)
        {
            Message = $"Таблица {tableName} базы {dbName} содержит более одной записи. Буфер не может содержать более одной записи";
        }
    }

    [Serializable]
    public class CutProductFromInputBufferException: InputBufferException
    {
        public override string Message { get; }
        public CutProductFromInputBufferException(Exception exception, string dbName, string tableName, string sn)
        {
            Message = $"Не удалось извлечь информацию об изделии c заводским номером {sn} из таблицы {tableName} базы {dbName}: {exception.Message}";
        }
    }

    [Serializable]
    public class ReadProductFromInputBufferException : InputBufferException
    {
        public override string Message { get; }
        public ReadProductFromInputBufferException(Exception exception, string dbName, string tableName, string id)
        {
            Message = $"Не удалось прочитать информацию об изделии c заводским номером/номером оснастки {id} из таблицы {tableName} базы {dbName}: {exception.Message}";
        }
    }


    [Serializable]
    public class ClearTableException : Exception
    {
        public override string Message { get; }
        public ClearTableException(Exception exception, string dbName, string tableName)
        {
            Message = $"Не удалось очистить таблицу {tableName} базы {dbName}: {exception.Message}";
        }
    }

    [Serializable]
    public class WorkTableException: Exception { }

    [Serializable]
    public class WriteToWorkTableException : WorkTableException
    {
        public override string Message { get; }
        public WriteToWorkTableException(Exception exception, string dbName, string tableName, string sn)
        {
            Message = $"Не удалось записать информацию об изделии c заводским номером {sn} в таблицу {tableName} базы {dbName}: {exception.Message}";
        }
    }

    [Serializable]
    public class CutProductFromWorkTableException : WorkTableException
    {
        public override string Message { get; }
        public CutProductFromWorkTableException(Exception exception, string dbName, string tableName, string sn)
        {
            Message = $"Не удалось извлечь информацию об изделии c заводским номером {sn} из таблицы {tableName} базы {dbName}: {exception.Message}";
        }
    }

    [Serializable]
    public class ReadProductFromWorkTableException : WorkTableException
    {
        public override string Message { get; }
        public ReadProductFromWorkTableException(Exception exception, string dbName, string tableName, string id)
        {
            Message = $"Не удалось прочитать информацию об изделии c заводским номером/номером оснастки {id} из таблицы {tableName} базы {dbName}: {exception.Message}";
        }
    }

    [Serializable]
    public class ResultTableException: Exception { }

    [Serializable]
    public class WriteProductToResultTableException: ResultTableException
    {
        public override string Message { get; }
        public WriteProductToResultTableException(Exception ex, string dbName, string tableName, string sn)
        {
            Message = $"Не удалось записать информацию об изделии c заводским номером {sn} в таблицу {tableName} базы {dbName}: {ex.Message}";
        }
    }

    [Serializable]
    public class WriteLogException : ResultTableException
    {
        public override string Message { get; }
        public WriteLogException(Exception ex, string dbName, string tableName, string sn)
        {
            Message = $"Не удалось прикрепить лог-файл калибровки изделия c заводским номером {sn} в таблицу {tableName} базы {dbName}: {ex.Message}";
        }
    }
}

