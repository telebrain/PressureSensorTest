﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureRack
{
    public static class Errors
    {
        public static string[] Messages { get; } =
        {
            "",
            "Не удалось установить связь со стойкой давления. Проверьте, включена ли стойка давления, " +
                "совпадает ли IP адрес СПК стойки давления и адрес, указанный в настройках", //1
            "Соединение со стойкой давления прервано", //2
            "Не удалось расшифровать принятые данные", //3
            "Команда не опознана стойкой давления", //4
            "Попытка изменения уставки при нахождении в очереди", //5
            "Запрос информации о несуществующем канале", //6
            "При изменении уставки задан несуществующий канал", //7
            "При изменении уставки задан канал, на котором находится неактивный контроллер", //8
            "При изменении уставки установлено значение, находящееся за границей диапазона контроллера", //9
            "Уставка больше, чем давление положительного источника", //10
            "Уставка меньше, чем давление отрицательного источника", //11
            "Внутренняя авария стойки давления. Превышен таймаут изменения уставки", //12
            "Неизвестный номер ошибки (13)",
            "Внутренняя авария стойки давления. Потеря связи с прибором переключения каналов", //14
            "Внутренняя авария стойки давления. Не удалось установить связь с запрашиваемым контроллером давления " +
                "из-за системной ошибки СПК", //15
            "Внутренняя авария стойки давления. Не удалось установить связь с запрашиваемым контроллером давления" //16
        };
    }
}
