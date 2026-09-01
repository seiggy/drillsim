using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OSDC.UnitConversion.Model
{
    public struct CountPerDay
    {
        public DateTime Date { get; set; }
        public ulong Count { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public CountPerDay() { }
        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="date"></param>
        /// <param name="count"></param>
        public CountPerDay(DateTime date, ulong count)
        {
            Date = date;
            Count = count;
        }
    }

    public class History 
    {
        public List<CountPerDay> Data { get; set; } = new List<CountPerDay>();
        /// <summary>
        /// default constructor
        /// </summary>
        public History() 
        {
            if (Data == null)
            {
                Data = new List<CountPerDay>();
            }
        }

        public void Increment()
        {
            if (Data.Count == 0)
            {
                Data.Add(new CountPerDay(DateTime.UtcNow.Date, 1));
            }
            else
            {
                if (Data[Data.Count - 1].Date < DateTime.UtcNow.Date)
                {
                    Data.Add(new CountPerDay(DateTime.UtcNow.Date, 1));
                }
                else
                {
                    Data[Data.Count - 1] = new CountPerDay(Data[Data.Count - 1].Date, Data[Data.Count - 1].Count + 1);
                }
            }
        }
    }
    public class UsageStatistics
    {
        public static readonly string HOME_DIRECTORY = ".." + Path.DirectorySeparatorChar + "home" + Path.DirectorySeparatorChar;

        public DateTime LastSaved { get; set; } = DateTime.MinValue;
        public TimeSpan BackUpInterval { get; set; } = TimeSpan.FromMinutes(5);

        public History PhysicalQuantityControllerGetAllIDPerDay { get; set; } = new History();
        public History PhysicalQuantityControllerGetAllPerDay { get; set; } = new History();
        public History PhysicalQuantityControllerGetByIDPerDay { get; set; } = new History();

        public History UnitConversionSetControllerGetAllIDPerDay { get; set; } = new History();
        public History UnitConversionSetControllerGetAllMetaInfoPerDay { get; set; } = new History();
        public History UnitConversionSetControllerGetAllPerDay { get; set; } = new History();
        public History UnitConversionSetControllerGetByIDPerDay { get; set; } = new History();
        public History UnitConversionSetControllerPostPerDay { get; set; } = new History();
        public History UnitConversionSetControllerPutPerDay { get; set; } = new History();
        public History UnitConversionSetControllerDeletePerDay { get; set; } = new History();

        public History UnitSystemControllerGetAllIDPerDay { get; set; } = new History();
        public History UnitSystemControllerGetAllLightPerDay { get; set; } = new History();
        public History UnitSystemControllerGetAllPerDay { get; set; } = new History();
        public History UnitSystemControllerGetByIDPerDay { get; set; } = new History();
        public History UnitSystemControllerPostPerDay { get; set; } = new History();
        public History UnitSystemControllerPutPerDay { get; set; } = new History();
        public History UnitSystemControllerDeletePerDay { get; set; } = new History();

        public History UnitSystemConversionSetControllerGetAllIDPerDay { get; set; } = new History();
        public History UnitSystemConversionSetControllerGetAllMetaInfoPerDay { get;  set; } = new History();
        public History UnitSystemConversionSetControllerGetAllPerDay { get; set; } = new History();
        public History UnitSystemConversionSetControllerGetByIDPerDay { get;  set; } = new History();
        public History UnitSystemConversionSetControllerPostPerDay { get;  set; } = new History();
        public History UnitSystemConversionSetControllerPutPerDay { get; set; } = new History();
        public History UnitSystemConversionSetControllerDeletePerDay { get; set; } = new History();

        private static object lock_ = new object();

        private static UsageStatistics? instance_ = null;

        public static UsageStatistics Instance
        {
            get
            {
                if (instance_ == null)
                {
                    if (File.Exists(HOME_DIRECTORY + "history.json"))
                    {
                        try
                        {
                            string? jsonStr = null;
                            lock (lock_)
                            {
                                using (StreamReader reader = new StreamReader(HOME_DIRECTORY + "history.json"))
                                {
                                    jsonStr = reader.ReadToEnd();
                                }
                                if (!string.IsNullOrEmpty(jsonStr))
                                {
                                    instance_ = JsonSerializer.Deserialize<UsageStatistics>(jsonStr);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    if (instance_ == null)
                    {
                        instance_ = new UsageStatistics();
                    }
                }
                return instance_;
            }
        }

        public void IncrementPhysicalQuantityControllerGetAllIDPerDay()
        {
            lock (lock_)
            {
                if (PhysicalQuantityControllerGetAllIDPerDay == null)
                {
                    PhysicalQuantityControllerGetAllIDPerDay = new History();
                }
                PhysicalQuantityControllerGetAllIDPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementPhysicalQuantityControllerGetAllPerDay()
        {
            lock (lock_)
            {
                if (PhysicalQuantityControllerGetAllPerDay == null)
                {
                    PhysicalQuantityControllerGetAllPerDay = new History();
                }
                PhysicalQuantityControllerGetAllPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementPhysicalQuantityControllerGetByIDPerDay()
        {
            lock (lock_)
            {
                if (PhysicalQuantityControllerGetByIDPerDay == null)
                {
                    PhysicalQuantityControllerGetByIDPerDay = new History();
                }
                PhysicalQuantityControllerGetByIDPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementUnitConversionSetControllerGetAllIDPerDay()
        {
            lock (lock_)
            {
                if (UnitConversionSetControllerGetAllIDPerDay == null)
                {
                    UnitConversionSetControllerGetAllIDPerDay = new History();
                }
                UnitConversionSetControllerGetAllIDPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitConversionSetControllerGetAllMetaInfoPerDay()
        {
            lock (lock_)
            {
                if (UnitConversionSetControllerGetAllMetaInfoPerDay == null)
                {
                    UnitConversionSetControllerGetAllMetaInfoPerDay = new History();
                }
                UnitConversionSetControllerGetAllMetaInfoPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitConversionSetControllerGetAllPerDay()
        {
            lock (lock_)
            {
                if (UnitConversionSetControllerGetAllPerDay == null)
                {
                    UnitConversionSetControllerGetAllPerDay = new History();
                }
                UnitConversionSetControllerGetAllPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitConversionSetControllerGetByIDPerDay()
        {
            lock (lock_)
            {
                if (UnitConversionSetControllerGetByIDPerDay == null)
                {
                    UnitConversionSetControllerGetByIDPerDay = new History();
                }
                UnitConversionSetControllerGetByIDPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitConversionSetControllerPostPerDay()
        {
            lock (lock_)
            {
                if (UnitConversionSetControllerPostPerDay == null)
                {
                    UnitConversionSetControllerPostPerDay = new History();
                }
                UnitConversionSetControllerPostPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitConversionSetControllerPutPerDay()
        {
            lock (lock_)
            {
                if (UnitConversionSetControllerPutPerDay == null)
                {
                    UnitConversionSetControllerPutPerDay = new History();
                }
                UnitConversionSetControllerPutPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitConversionSetControllerDeletePerDay()
        {
            lock (lock_)
            {
                if (UnitConversionSetControllerDeletePerDay == null)
                {
                    UnitConversionSetControllerDeletePerDay = new History();
                }
                UnitConversionSetControllerDeletePerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemControllerGetAllIDPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemControllerGetAllIDPerDay == null)
                {
                    UnitSystemControllerGetAllIDPerDay = new History();
                }
                UnitSystemControllerGetAllIDPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementUnitSystemControllerGetAllPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemControllerGetAllPerDay == null)
                {
                    UnitSystemControllerGetAllPerDay = new History();
                }
                UnitSystemControllerGetAllPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemControllerGetAllLightPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemControllerGetAllLightPerDay == null)
                {
                    UnitSystemControllerGetAllLightPerDay = new History();
                }
                UnitSystemControllerGetAllLightPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemControllerGetByIDPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemControllerGetByIDPerDay == null)
                {
                    UnitSystemControllerGetByIDPerDay = new History();
                }
                UnitSystemControllerGetByIDPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemControllerPostPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemControllerPostPerDay == null)
                {
                    UnitSystemControllerPostPerDay = new History();
                }
                UnitSystemControllerPostPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemControllerPutPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemControllerPutPerDay == null)
                {
                    UnitSystemControllerPutPerDay = new History();
                }
                UnitSystemControllerPutPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemControllerDeletePerDay()
        {
            lock (lock_)
            {
                if (UnitSystemControllerDeletePerDay == null)
                {
                    UnitSystemControllerDeletePerDay = new History();
                }
                UnitSystemControllerDeletePerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementUnitSystemConversionSetControllerGetAllIDPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemConversionSetControllerGetAllIDPerDay == null)
                {
                    UnitSystemConversionSetControllerGetAllIDPerDay = new History();
                }
                UnitSystemConversionSetControllerGetAllIDPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemConversionSetControllerGetAllMetaInfoPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemConversionSetControllerGetAllMetaInfoPerDay == null)
                {
                    UnitSystemConversionSetControllerGetAllMetaInfoPerDay = new History();
                }
                UnitSystemConversionSetControllerGetAllMetaInfoPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemConversionSetControllerGetAllPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemConversionSetControllerGetAllPerDay == null)
                {
                    UnitSystemConversionSetControllerGetAllPerDay = new History();
                }
                UnitSystemConversionSetControllerGetAllPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemConversionSetControllerGetByIDPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemConversionSetControllerGetByIDPerDay == null)
                {
                    UnitSystemConversionSetControllerGetByIDPerDay = new History();
                }
                UnitSystemConversionSetControllerGetByIDPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemConversionSetControllerPostPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemConversionSetControllerPostPerDay == null)
                {
                    UnitSystemConversionSetControllerPostPerDay = new History();
                }
                UnitSystemConversionSetControllerPostPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemConversionSetControllerPutPerDay()
        {
            lock (lock_)
            {
                if (UnitSystemConversionSetControllerPutPerDay == null)
                {
                    UnitSystemConversionSetControllerPutPerDay = new History();
                }
                UnitSystemConversionSetControllerPutPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementUnitSystemConversionSetControllerDeletePerDay()
        {
            lock (lock_)
            {
                if (UnitSystemConversionSetControllerDeletePerDay == null)
                {
                    UnitSystemConversionSetControllerDeletePerDay = new History();
                }
                UnitSystemConversionSetControllerDeletePerDay.Increment();
                ManageBackup();
            }
        }

        private void ManageBackup()
        {
            if (DateTime.UtcNow > LastSaved + BackUpInterval)
            {
                LastSaved = DateTime.UtcNow;
                try
                {
                    string jsonStr = JsonSerializer.Serialize(this);
                    if (!string.IsNullOrEmpty(jsonStr) && Directory.Exists(HOME_DIRECTORY))
                    {
                        using (StreamWriter writer = new StreamWriter(HOME_DIRECTORY + "history.json"))
                        {
                            writer.Write(jsonStr);
                            writer.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
