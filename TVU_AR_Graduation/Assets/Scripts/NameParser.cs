using System.Linq;

public static class NameParser
{
    public static string ExtractStudentName(string ocrText)
    {
        if (string.IsNullOrEmpty(ocrText))
        {
            return "SINH VIÊN";
        }
        
        // Tách thành các dòng và clean
        string[] lines = ocrText.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        
        // Tìm dòng chứa "Cử nhân" hoặc "CỬ NHÂN"
        for (int i = 0; i < lines.Length - 1; i++)
        {
            string line = lines[i].ToUpper();
            if (line.Contains("CỬ NHÂN") || line.Contains("CU NHAN"))
            {
                // Lấy dòng tiếp theo (thường là tên)
                string nameCandidate = lines[i + 1];
                
                // Validate: Tên phải có ít nhất 2 từ và không chứa từ khóa trường/khoa
                if (IsValidName(nameCandidate))
                {
                    return nameCandidate.ToUpper();
                }
            }
        }
        
        // Fallback: Tìm dòng có nhiều chữ cái và ít nhất 2 từ
        foreach (string line in lines)
        {
            if (IsValidName(line))
            {
                return line.ToUpper();
            }
        }
        
        // Last fallback
        return "SINH VIÊN";
    }
    
    private static bool IsValidName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
            
        // Loại bỏ các từ khóa không phải tên
        string upper = text.ToUpper();
        if (upper.Contains("TRƯỜNG") || upper.Contains("TRUONG") ||
            upper.Contains("ĐẠI HỌC") || upper.Contains("DAI HOC") ||
            upper.Contains("KHOA") || upper.Contains("CỬ NHÂN") ||
            upper.Contains("CU NHAN") || upper.Contains("TRÀ VINH") ||
            upper.Contains("TRA VINH"))
        {
            return false;
        }
        
        // Phải có ít nhất 2 từ
        string[] words = text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2)
            return false;
            
        // Phải có ít nhất 6 ký tự
        if (text.Length < 6)
            return false;
            
        // Phải có nhiều chữ cái
        int letterCount = text.Count(char.IsLetter);
        return letterCount >= 6;
    }
}
