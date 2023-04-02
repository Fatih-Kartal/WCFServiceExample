using System.Text;

namespace WebApplication.Helpers
{
    public static class LogDrawer
    {
        public static string Draw(string s)
        {
            string corner = "+";
            string edge = "-";

            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


            int longest = 0;
            foreach (string line in lines)
            {
                if (line.Length > longest)
                    longest = line.Length;
            }
            int width = longest + 2; // 1 space on each side


            string h = string.Empty;
            for (int i = 0; i < width; i++)
                h += edge;

            // box top
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(corner + h + corner);

            // box contents
            foreach (string line in lines)
            {
                // add the text line to the box
                sb.AppendLine(" " + line);
            }

            // box bottom
            sb.AppendLine(corner + h + corner);

            // the finished box
            return sb.ToString();
        }
    }
}
