using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Zcab.Models
{
    public class FilesDownloader
    {
        public List<ZFiles> files;
        public ZFiles file;
        public WorkDoc doc;
        public string warning;

        /// <summary>
        /// Метод для загрузки файла из БД
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fileId"></param>
        /// <returns>RetOk</returns>
        public RetOk getAttachedFile(int userId, int fileId)
        {
            // Специальный класс для обработки ошибок
            RetOk rok = new RetOk();
            Downloader downloader = new Downloader();

            try
            {
                downloader.StartConn();
                file = downloader.getAttachedFile(userId, fileId);
                downloader.EndConn();
            }
            catch (Exception ex)
            {
                downloader.EndConn();
                rok.Error(ex, "Загрузка файла по заявке");
            }
            return rok;
        }
    }

    /// <summary>
    /// Класс для "верхнеуровнего" взаимодействия с БД
    /// </summary>
    public class Downloader : DB_dr
    {
        public Downloader()
        {
            ClearParams = true;
        }

        /// <summary>
        /// Создаёт объект ZFiles из результата выполнения хранимой процедуры
        /// </summary>
        /// <param name="r">Представляет собой параметр типа, который предназначен для хранения
        /// значения полей в виде массива object</param>
        /// <returns>ZFiles</returns>
        public ZFiles formAttachedFile(RowIncRead r)
        {
            ZFiles file = new ZFiles();

            file.Id = r.Int;
            file.Name = r.Str;
            file.Title = r.Str;
            file.Data = r.ByteArr;
            file.Date = r.Date;

            return file;
        }

        /// <summary>
        /// Получает файл из БД при помощи хранимой процедуры
        /// </summary>
        /// <param name="uid">id пользователя</param>
        /// <param name="id">id файла</param>
        /// <returns>ZFiles</returns>
        public ZFiles getAttachedFile(int uid, int id)
        {
            string procedure = "lk.V_AttachedFileGet";
            Par("user_id", uid);
            Par("file_id", id);
            return ReadByProc<ZFiles>(procedure, formAttachedFile);
        }
    }
}
