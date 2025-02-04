using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMTestConnectorLib.Models
{
    public class Candle
    {
        /// <summary>
        /// Валютная пара
        /// </summary>
        public string Pair { get; set; }

        /// <summary>
        /// Цена открытия
        /// </summary>
        public decimal OpenPrice { get; set; }

        /// <summary>
        /// Максимальная цена
        /// </summary>
        public decimal HighPrice { get; set; }

        /// <summary>
        /// Минимальная цена
        /// </summary>
        public decimal LowPrice { get; set; }

        /// <summary>
        /// Цена закрытия
        /// </summary>
        public decimal ClosePrice { get; set; }


        /// <summary>
        /// Partial (Общая сумма сделок)
        /// </summary>
        //public decimal TotalPrice { get; set; } //убрал, потому что api не дает информацию о сумме вместе с остальным запросом и для получения общей суммы нужно делать
                                                  //два отдельных запроса для short и long и информация доступна не для всех symbols

        /// <summary>
        /// Partial (Общий объем)
        /// </summary>
        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Время
        /// </summary>
        public DateTimeOffset OpenTime { get; set; }

    }
}
