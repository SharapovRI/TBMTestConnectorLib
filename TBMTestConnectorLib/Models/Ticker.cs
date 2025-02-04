using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMTestConnectorLib.Models
{
    public class Ticker
    {
        /// <summary>
        /// Цена самой высокой заявки на покупку
        /// </summary>
        public decimal Bid { get; set; }

        /// <summary>
        /// Сумма 25 самых высоких заявок на покупку
        /// </summary>
        public decimal BidSize { get; set; }

        /// <summary>
        /// Цена самой высокой заявки на продажу
        /// </summary>
        public decimal Ask { get; set; }

        /// <summary>
        /// Сумма 25 самых высоких заявок на продажу
        /// </summary>
        public decimal AskSize { get; set; }
        
        /// <summary>
        /// Сумма на которую изменилась цена со вчерашнего дня
        /// </summary>
        public decimal DailyChange { get; set; }

        /// <summary>
        /// Относительное изменение цены со вчерашнего дня в процентном соотношении
        /// </summary>
        public decimal DailyChangeRelative { get; set; }
        
        /// <summary>
        /// Цена последней сделки
        /// </summary>
        public decimal LastPrice { get; set; }
        
        /// <summary>
        /// Объем за день
        /// </summary>
        public decimal Volume { get; set; }
        
        /// <summary>
        /// Высшая цена за день
        /// </summary>
        public decimal High { get; set; }
        
        /// <summary>
        /// Низшая цена за день
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Валютная пара
        /// </summary>
        public string Pair { get; set; }
    }
}
