using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Model
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
    public class UsageStatisticsCartographicProjection
    {
        public static readonly string HOME_DIRECTORY = ".." + Path.DirectorySeparatorChar + "home" + Path.DirectorySeparatorChar;

        public DateTime LastSaved { get; set; } = DateTime.MinValue;
        public TimeSpan BackUpInterval { get; set; } = TimeSpan.FromMinutes(5);

        public History GetAllCartographicProjectionTypeIdPerDay { get; set; } = new History();
        public History GetCartographicProjectionTypeByIdPerDay { get; set; } = new History();
        public History GetAllCartographicProjectionTypePerDay { get; set; } = new History();

        public History GetAllCartographicProjectionIdPerDay { get; set; } = new History();
        public History GetAllCartographicProjectionMetaInfoPerDay { get; set; } = new History();
        public History GetCartographicProjectionByIdPerDay { get; set; } = new History();
        public History GetAllCartographicProjectionLightPerDay { get; set; } = new History();
        public History GetAllCartographicProjectionPerDay { get; set; } = new History();
        public History PostCartographicProjectionPerDay { get; set; } = new History();
        public History PutCartographicProjectionByIdPerDay { get; set; } = new History();
        public History DeleteCartographicProjectionByIdPerDay { get; set; } = new History();

        public History GetAllCartographicConversionSetIdPerDay { get; set; } = new History();
        public History GetAllCartographicConversionSetMetaInfoPerDay { get; set; } = new History();
        public History GetCartographicConversionSetByIdPerDay { get; set; } = new History();
        public History GetAllCartographicConversionSetLightPerDay { get; set; } = new History();
        public History GetAllCartographicConversionSetPerDay { get; set; } = new History();
        public History PostCartographicConversionSetPerDay { get; set; } = new History();
        public History PutCartographicConversionSetByIdPerDay { get; set; } = new History();
        public History DeleteCartographicConversionSetByIdPerDay { get; set; } = new History();

        private static object lock_ = new object();

        private static UsageStatisticsCartographicProjection? instance_ = null;

        public static UsageStatisticsCartographicProjection Instance
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
                                    instance_ = JsonSerializer.Deserialize<UsageStatisticsCartographicProjection>(jsonStr);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    if (instance_ == null)
                    {
                        instance_ = new UsageStatisticsCartographicProjection();
                    }
                }
                return instance_;
            }
        }

        public void IncrementGetAllCartographicProjectionTypeIdPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicProjectionTypeIdPerDay == null)
                {
                    GetAllCartographicProjectionTypeIdPerDay = new History();
                }
                GetAllCartographicProjectionTypeIdPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementGetCartographicProjectionTypeByIdPerDay()
        {
            lock (lock_)
            {
                if (GetCartographicProjectionTypeByIdPerDay == null)
                {
                    GetCartographicProjectionTypeByIdPerDay = new History();
                }
                GetCartographicProjectionTypeByIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicProjectionTypePerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicProjectionTypePerDay == null)
                {
                    GetAllCartographicProjectionTypePerDay = new History();
                }
                GetAllCartographicProjectionTypePerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementGetAllCartographicProjectionIdPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicProjectionIdPerDay == null)
                {
                    GetAllCartographicProjectionIdPerDay = new History();
                }
                GetAllCartographicProjectionIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicProjectionMetaInfoPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicProjectionMetaInfoPerDay == null)
                {
                    GetAllCartographicProjectionMetaInfoPerDay = new History();
                }
                GetAllCartographicProjectionMetaInfoPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetCartographicProjectionByIdPerDay()
        {
            lock (lock_)
            {
                if (GetCartographicProjectionByIdPerDay == null)
                {
                    GetCartographicProjectionByIdPerDay = new History();
                }
                GetCartographicProjectionByIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicProjectionLightPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicProjectionLightPerDay == null)
                {
                    GetAllCartographicProjectionLightPerDay = new History();
                }
                GetAllCartographicProjectionLightPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementPostCartographicProjectionPerDay()
        {
            lock (lock_)
            {
                if (PostCartographicProjectionPerDay == null)
                {
                    PostCartographicProjectionPerDay = new History();
                }
                PostCartographicProjectionPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicProjectionPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicProjectionPerDay == null)
                {
                    GetAllCartographicProjectionPerDay = new History();
                }
                GetAllCartographicProjectionPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementPutCartographicProjectionByIdPerDay()
        {
            lock (lock_)
            {
                if (PutCartographicProjectionByIdPerDay == null)
                {
                    PutCartographicProjectionByIdPerDay = new History();
                }
                PutCartographicProjectionByIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementDeleteCartographicProjectionByIdPerDay()
        {
            lock (lock_)
            {
                if (DeleteCartographicProjectionByIdPerDay == null)
                {
                    DeleteCartographicProjectionByIdPerDay = new History();
                }
                DeleteCartographicProjectionByIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicConversionSetIdPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicConversionSetIdPerDay == null)
                {
                    GetAllCartographicConversionSetIdPerDay = new History();
                }
                GetAllCartographicConversionSetIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicConversionSetMetaInfoPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicConversionSetMetaInfoPerDay == null)
                {
                    GetAllCartographicConversionSetMetaInfoPerDay = new History();
                }
                GetAllCartographicConversionSetMetaInfoPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementGetCartographicConversionSetByIdPerDay()
        {
            lock (lock_)
            {
                if (GetCartographicConversionSetByIdPerDay == null)
                {
                    GetCartographicConversionSetByIdPerDay = new History();
                }
                GetCartographicConversionSetByIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicConversionSetLightPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicConversionSetLightPerDay == null)
                {
                    GetAllCartographicConversionSetLightPerDay = new History();
                }
                GetAllCartographicConversionSetLightPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementGetAllCartographicConversionSetPerDay()
        {
            lock (lock_)
            {
                if (GetAllCartographicConversionSetPerDay == null)
                {
                    GetAllCartographicConversionSetPerDay = new History();
                }
                GetAllCartographicConversionSetPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementPostCartographicConversionSetPerDay()
        {
            lock (lock_)
            {
                if (PostCartographicConversionSetPerDay == null)
                {
                    PostCartographicConversionSetPerDay = new History();
                }
                PostCartographicConversionSetPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementPutCartographicConversionSetByIdPerDay()
        {
            lock (lock_)
            {
                if (PutCartographicConversionSetByIdPerDay == null)
                {
                    PutCartographicConversionSetByIdPerDay = new History();
                }
                PutCartographicConversionSetByIdPerDay.Increment();
                ManageBackup();
            }
        }
        public void IncrementDeleteCartographicConversionSetByIdPerDay()
        {
            lock (lock_)
            {
                if (DeleteCartographicConversionSetByIdPerDay == null)
                {
                    DeleteCartographicConversionSetByIdPerDay = new History();
                }
                DeleteCartographicConversionSetByIdPerDay.Increment();
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
