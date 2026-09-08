using System;
using System.Collections.Generic;

namespace OPDQueueSimulator
{
    
    public static class OpdDataset
    {
        public static List<Patient> Load()
        {
            (int serial, string name, string arr, string start, string end)[] raw =
            {
                (1,  "Asma Khatoon",             "08:20", "10:30", "10:34"),
                (2,  "Shah Wazeer",               "08:22", "10:35", "10:37"),
                (3,  "Muhammad Khan",              "08:23", "10:37", "10:41"),
                (4,  "Ghulam Nabi",                "08:25", "10:41", "10:43"),
                (5,  "Nadeem Shafqat",             "08:26", "10:44", "10:46"),
                (6,  "Tahir Ahmed",                "08:35", "10:46", "10:48"),
                (7,  "Ghulam Muhammad",            "08:47", "10:48", "10:53"),
                (8,  "Zareena",                    "08:56", "10:54", "10:56"),
                (9,  "Abdul Tahir Abdul Hakeem",   "08:59", "10:57", "10:59"),
                (10, "Hameeda Begum",               "09:11", "10:59", "11:01"),
                (11, "Shahzal",                     "09:14", "11:01", "11:04"),
                (12, "Manthar Jafar",               "09:15", "11:05", "11:07"),
                (13, "Sadia Bibi",                  "09:21", "11:08", "11:10"),
                (14, "Muhammad Yar",                "09:24", "11:12", "11:15"),
                (15, "Imran Ahmed",                 "09:31", "11:15", "11:18"),
                (16, "Ali Ahmed",                    "09:37", "11:16", "11:20"),
                (17, "Zuli Khan",                   "09:51", "11:20", "11:22"),
                (18, "Abdul Majeed",                "09:55", "11:23", "11:26"),
                (19, "Ayesha Begum",                "10:01", "11:27", "11:29"),
                (20, "Bilal Khan",                  "10:07", "11:30", "11:33"),
            };

            var list = new List<Patient>();
            foreach (var r in raw)
            {
                list.Add(new Patient
                {
                    SerialNo = r.serial,
                    Name = r.name,
                    ArrivalTime = TimeSpan.Parse(r.arr),
                    ActualServiceStart = TimeSpan.Parse(r.start),
                    ActualServiceEnd = TimeSpan.Parse(r.end)
                });
            }
            return list;
        }
    }
}
