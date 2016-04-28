using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Airline.AP.Airline.Model
{
    public abstract class AirlineObject : IAirlineObjectView//IAirlineObject, 
    {
        
        public Guid ID { get; set; }
        

        public abstract string[] TableHeader { get; }

        public abstract string[,] TableView { get; }

        protected const int tableRowCount = 2;
        
        public object this[string propertyName]
        {
            get { return GetType().GetProperty(propertyName).GetValue(this, null); }
            set { GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }

        /// <summary>
        /// Shows the content of the object in table format
        /// </summary>
        /// <returns>String table view</returns>
        public virtual string Show()
        {
            return ToStringTable(TableView);
        }

        /// <summary>
        /// Turns airline object properties to an string table
        /// </summary>
        /// <param name="arrayValues">Array of values to display</param>
        /// <returns>String view</returns>
        protected string ToStringTable(string[,] arrayValues)
        {
            int[] maxColumnsWidth = GetMaxColumnWidth(arrayValues);
            if (maxColumnsWidth.Length > 0)
            {
                var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

                var sb = new StringBuilder();
                for (int rowIndex = 0; rowIndex < arrayValues.GetLength(0); rowIndex++)
                {
                    for (int colIndex = 0; colIndex < arrayValues.GetLength(1); colIndex++)
                    {
                        if (arrayValues[rowIndex, colIndex] != null)
                        {
                            // Print cell
                            string cell = arrayValues[rowIndex, colIndex];
                            cell = cell.PadRight(maxColumnsWidth[colIndex]);
                            sb.Append(" | ");
                            sb.Append(cell);
                        }
                    }


                    if (arrayValues[rowIndex, 0] != null)
                    {
                        // Print end of line
                        sb.Append(" | ");
                        sb.AppendLine();

                        // Print splitter
                        if (rowIndex == 0)
                        {
                            sb.AppendFormat(" |{0}| ", headerSpliter);
                            sb.AppendLine();
                        }
                    }
                }

                return sb.ToString();
            }
            return "";
        }

        /// <summary>
        /// Calculates table's column width
        /// </summary>
        /// <param name="arrayValues">Array of values to calculate</param>
        /// <returns>Array of column with their width</returns>
        private int[] GetMaxColumnWidth(string[,] arrayValues)
        {
            var maxColumnsWidth = new int[arrayValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrayValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrayValues.GetLength(0); rowIndex++)
                {
                    if (arrayValues[rowIndex, colIndex] != null)
                    {
                        int newLength = arrayValues[rowIndex, colIndex].Length;
                        int oldLength = maxColumnsWidth[colIndex];

                        if (newLength > oldLength)
                        {
                            maxColumnsWidth[colIndex] = newLength;
                        }
                    }
                }
            }

            return maxColumnsWidth;
        }

        public abstract bool IsValid();
    }
}
