using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleExcelClosedXML;


namespace OwenPressureDevices
{
    public class MetrologicGroups
    {
        XlFileReader xlFileReader;

        bool fileWasLoaded = false;
        readonly int standID = -1;

        public MetrologicGroups(int standID)
        {
            this.standID = standID;
        }

        private void LoadFile()
        {
            try
            {
                xlFileReader = new XlFileReader();
                xlFileReader.LoadFile("MetrologicGroups.xlsx");
                fileWasLoaded = true;
            }
            catch(Exception ex)
            {
                throw new MetrologicGroupFileNotFoundException(ex.Message);
            }
        }

        public int GetMetrologicGroup(string deviceName)
        {
            if (!fileWasLoaded)
                LoadFile();
            var nameInfo = ParserNamePD100.ParseName(deviceName);
            int firsyRow = GetFirstRow(standID);
            int typeRow = GetTypeRow(firsyRow, nameInfo.RangeType);
            var groups = GetGroupsByRange(typeRow);
            foreach(var group in groups)
            {
                if (group.CheckRange(nameInfo.PressureRange))
                    return group.GroupId;
            }
            throw new MetrologicGroupNotFoundException();
        }

        private int GetFirstRow(int standID)
        {
            for(int i = 2; i < xlFileReader.RowsUsed; i++)
            {
                int? value = GetStandId(i);
                if (value != null && value.Value == standID)
                    return i;
            }
            return -1;
        }

        private int GetTypeRow(int firstRow, string rangeType)
        {
            for (int i = firstRow; i <= xlFileReader.RowsUsed; i++)
            {
                // Не следующий ли стенд
                if (i > firstRow && GetStandId(i) != null)
                    throw new MetrologicGroupNotFoundException();
                if (CheckRangeType(i, rangeType))
                    return i;
            }
            throw new MetrologicGroupNotFoundException();
        }

        private bool CheckRangeType(int row, string rangeType)
        {
            string[] groupType = GetGroupType(row);
            if (groupType == null)
                return false;
            return !(Array.IndexOf(groupType, rangeType) < 0);
        }

        private List<GroupByRange> GetGroupsByRange(int firstRow)
        {
            var groupByRange = new List<GroupByRange>();
            for (int i = firstRow; i <= xlFileReader.RowsUsed; i++)
            {
                // Не следующий ли тип
                if (i > firstRow && GetGroupType(i) != null)
                    break;
                groupByRange.Add(new GroupByRange(GetGroupRange(i), GetGroupId(i)));
            }
            groupByRange.Sort();
            return groupByRange;
        }

        private int? GetStandId(int row)
        {
            object cell = xlFileReader.GetCell(row, 1);
            if (cell == null || !int.TryParse(cell.ToString(), out int value))
                return null;
            return value;
        }

        private string[] GetGroupType(int row)
        {
            object cell = xlFileReader.GetCell(row, 2);
            if (cell == null)
                return null;
            string value = cell.ToString();
            if (string.IsNullOrEmpty(value))
                return null;
            if (value.IndexOf(',') < 0)
                return new string[] { value };
            string[] result = value.Split((new string[] { ", "}), StringSplitOptions.RemoveEmptyEntries) ;
            if (result == null)
                throw new MetrologicGroupFileFormatException(row, 2);
            return result;
        }

        private float GetGroupRange(int row)
        {
            object cell = xlFileReader.GetCell(row, 3);
            if (cell == null || !float.TryParse(cell.ToString(), out float value))
                throw new MetrologicGroupFileFormatException(row, 3);
            return value;
        }

        private int GetGroupId(int row)
        {
            object cell = xlFileReader.GetCell(row, 4);
            if (cell == null || !int.TryParse(cell.ToString(), out int value))
                throw new MetrologicGroupFileFormatException(row, 4);
            return value;
        }
    }


    public class GroupByRange: IComparable
    {
        public int MaxRange { get; private set; } // Максимальный диапазон в Па

        public int GroupId { get; private set; }

        public GroupByRange(float maxRange, int groupID)
        {
            MaxRange = (int)(maxRange*1000); // 
            GroupId = groupID;
        }

        public bool CheckRange(string rangeLabel)
        {
            if (!float.TryParse(rangeLabel, out float deviceRange))
                throw new MetrologicGroupNotFoundException();
            return (int)(deviceRange * 1e+6) <= MaxRange;
        }

        
        public int CompareTo(object obj)
        {
            int maxRange = ((GroupByRange)obj).MaxRange;
            if (MaxRange > maxRange)
                return 1;
            else if (MaxRange < maxRange)
                return -1;
            else
                return 0;
        }
    }

    public class MetrologicGroupFileNotFoundException: Exception
    {
        public MetrologicGroupFileNotFoundException(string message): base($"Не найден файл метрологических групп: {message}")
        { }
    }

    public class StandIdNotFoundException : Exception
    {
        public StandIdNotFoundException(int standID) : base($"ID стенда {standID} не найден в файле метрологических групп")
        { }
    }

    public class MetrologicGroupNotFoundException : Exception
    {
        public MetrologicGroupNotFoundException() : base($"Для поверяемого изделия не удалось определить метрологическую группу")
        { }
    }

    public class MetrologicGroupFileFormatException : Exception
    {
        public MetrologicGroupFileFormatException(int row, int column) : 
            base($"Ошибка формата файла метрологических групп. Строка {row}, столбец {column}")
        { }
    }
}
