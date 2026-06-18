public interface IFileService
{
    string SaveFile(IFormFile file);
}

public class FileService : IFileService
{
    public string SaveFile(IFormFile file)
    {
        if (file == null) return null;

        // הגדרת הנתיב לתיקיית הגלובוס
        var folderName = Path.Combine("wwwroot", "images", "gifts");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        // יצירת התיקייה אם היא לא קיימת
        if (!Directory.Exists(pathToSave)) Directory.CreateDirectory(pathToSave);

        // יצירת שם ייחודי לקובץ
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var fullPath = Path.Combine(pathToSave, fileName);

        // שמירת הקובץ בשרת
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        // מחזירים את המחרוזת (string) שתישמר ב-DB
        return "/images/gifts/" + fileName;
    }
}
