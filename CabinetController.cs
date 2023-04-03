using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Zcab.Models;
using Newtonsoft.Json;

namespace Zcab.Controllers
{
    public class CabController : Controller
    {
        /// <summary>
        /// Обрабатывает запрос на получение прикреплённого к сообщению чата файла
        /// </summary>
        /// <param name="id">id файла</param>
        /// <returns>ActionResult</returns>
        public ActionResult GetAttached(int id)
        {
            // Специальный класс для обработки ошибок
            RetOk rok = new RetOk();

            FilesDownloader filesDownloader = new FilesDownloader();
            rok = filesDownloader.getAttachedFile((int)LkWork.UserID, id);

            // В случае ошибки при получении файла из БД возвращается страница с сообщением об ошибке
            if (!rok) return PageError(rok);

            ZFiles file = filesDownloader.file;

            // Аналогично для null
            if (file == null) return PageError(new RetOk("Ошибка при скачивании файла!"));
            return File(file.Data, Gwork.GetFileContentType(file.Name), file.Name);
        }
    }

    /// <summary>
    /// Обрабатывает запрос на получение строки адреса объекта, как она хранится в росреестре, по кадастровому номеру
    /// </summary>
    /// <param name="cad_num">Кадастровый номер объекта</param>
    /// <param name="type">Тип, по которому осуществлять поиск: ОКС или земельный участок</param>
    /// <returns>Task<string></returns>
    [HttpPost]
    public async Task<string> GetAddressFromRosreestr(string cad_num, int type)
    {
        RosreestrLinker rosreestr = new RosreestrLinker { cad_num = cad_num, req_type = type };
        await rosreestr.SendRequest();

        return rosreestr.address;
    }
}