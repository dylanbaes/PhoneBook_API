using PhoneBook.Exceptions;
using PhoneBook.Model;
using System.IO;
using System.Text.RegularExpressions;

namespace PhoneBook.Services
{
    public class DictionaryPhoneBookService : IPhoneBookService
    {
        private readonly Dictionary<string, string> _phoneBookEntries;
        private readonly string _csvFilePath = Directory.GetCurrentDirectory() + ".csv";
        private readonly string _auditLogFilePath = Directory.GetCurrentDirectory() + "_auditlog.txt";

        public DictionaryPhoneBookService()
        {
            _phoneBookEntries = new Dictionary<string, string>();
            LoadDataFromCsv();
        }

        public void Add(PhoneBookEntry phoneBookEntry)
        {
            if (phoneBookEntry.Name == null ||  phoneBookEntry.PhoneNumber == null)
            {
                throw new ArgumentException("Name and phone number must both be specified.");
            }

            // Name Validation
            
            string phoneBookName = phoneBookEntry.Name;
            string phoneBookPhoneNumber = phoneBookEntry.PhoneNumber;
            
            // Condition checks for integers in the entry
            if (phoneBookName.Any(char.IsDigit))
            {
                throw new ArgumentException("Please provide a valid name with no numbers.");
            }

            // Condition checks for if there are more than 3 names that have been entered, separated by white spaces
            string[] numNames = phoneBookName.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (numNames.Length > 3)
            {
                throw new ArgumentException("Please provide a valid name with 3 or less names separated by white spaces");
            }
            // Condition checks for # of "'", fails if occurs more than once.
            if (phoneBookName.Contains('\'') || phoneBookName.Contains('’'))
            {
                bool seen = false;
                char apost = '\'';
                char apost2 = '’';
                for (int i = 0; i < phoneBookName.Length; i++)
                {
                    if (phoneBookName[i] != apost && phoneBookName[i] != apost2)
                    {
                        continue;
                    }
                    if (seen)
                    {
                        throw new ArgumentException("Please provide a valid name with only one apostrophe.");
                    }
                    seen = true;
                }
            }
            // Condition checks for # of "-", fails if occurs more than once.
            if (phoneBookName.Contains('-'))
            {
                bool seen = false;
                char hyphen = '-';
                for (int i = 0; i < phoneBookName.Length; i++)
                {
                    if (phoneBookName[i] != hyphen)
                    {
                        continue;
                    }
                    if (seen)
                    {
                        throw new ArgumentException("Please provide a valid name with only one hyphen.");
                    }
                    seen = true;
                }
            }

            if (phoneBookName.Contains(','))
            {
                string[] names = phoneBookName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                phoneBookEntry.Name = names[1].Trim() + " " + names[0].Trim();
            }

            // Phone Number Validation            
                                   
            string regex_area_dash_sub = @"^\d{3}-\d{3}-\d{4}$"; // <Area Code>-<Subscriber Number> (e.g. 670-123-4567)
            string regex_area_bracket = @"^\(\d{3}\)\d{3}-\d{4}$"; // (<Area Code>)<Subscriber Number> (e.g. (670)123-4567)
            string regex_one_dash = @"^1-\d{3}-\d{3}-\d{4}$"; // 1-<Area Code>-<Subscriber Number> (e.g. 1-670-123-4567)
            string regex_one_bracket = @"^1\(\d{3}\)\d{3}-\d{4}$"; // 1(<Area Code>)<Subscriber Number> (e.g. 1(670)123-4567)
            string regex_area_space = @"^\d{3}\s\d{3}\s\d{4}$"; // <Area Code> <Subscriber Number> (e.g. 670 123 4567)
            string regex_periods = @"^\d{3}\.\d{3}\.\d{4}$"; // <Area Code>.<Subscriber Number> (e.g. 670.123.4567)
            string regex_one_space = @"^\+?1\s\d{3}\s\d{3}\s\d{4}$"; // 1 <Area Code> <Subscriber Number> (e.g. 1 670 123 4567) can contain +
            string regex_one_period = @"^\+?1\.\d{3}\.\d{3}\.\d{4}$"; // 1.<Area Code>.<Subscriber Number> (e.g. 1.670.123.4567) can contain +
            string regex_7_subnum = @"^\d{3}-\d{4}$"; // <Subscriber Number> (e.g. 123-4567)
            string ext_regex = @"^[0-9]{5}$"; // 12345
            string regex_threes = @"^\d{3}\s\d{3}\s\d{3}\s\d{4}$"; // 011 701 111 1234 format
            string regex_fives = @"^\d{5}\.\d{5}$"; // 12345.12345 format
            string regex_three_one = @"^\d{1,3}\s\d{1,3}\s\d{3}\s\d{3}\s\d{4}$"; // 011 1 703 111 1234 format
            string regex_plus_one_brackets = @"^\+?\d{1}\(\d{3}\)\d{3}-\d{4}$"; // +1(703)111-2121 format
            string regex_long_country = @"^\+?\d{1,2}\s\(\d{1,2}\)\s\d{3}-\d{4}$"; // +32 (21) 212-2324 format

            // Danish Regex
            string regex_two_four = @"^(\+?45)?\d{2}[ .]\d{2}[ .]\d{2}[ .]\d{2}$"; // AA AA AA AA or AA.AA.AA.AA with optional +45 at beginning
            string regex_four_two = @"^(\+?45)?\d{4}[ .]\d{4}$"; // AAAA AAAA or AAAA.AAAA with optional +45 at beginning
            // Validation for 5-digit extension

            if (!Regex.IsMatch(phoneBookPhoneNumber, ext_regex) && phoneBookPhoneNumber.Length < 6)
            {
                throw new ArgumentException("Did not match Regex for 5-digit extension");
            }

            // Validation for 7-digit subscriber number

            if (!Regex.IsMatch(phoneBookPhoneNumber, regex_7_subnum) && phoneBookPhoneNumber.Length == 8)
            {
                throw new ArgumentException("Did not match Regex for 7-digit subscriber number");
            }

            // Validation for all other formats
            if ((!Regex.IsMatch(phoneBookPhoneNumber, regex_area_dash_sub) && !Regex.IsMatch(phoneBookPhoneNumber, regex_area_bracket) &&
                !Regex.IsMatch(phoneBookPhoneNumber, regex_one_dash) && !Regex.IsMatch(phoneBookPhoneNumber, regex_one_bracket) &&
                !Regex.IsMatch(phoneBookPhoneNumber, regex_area_space) && !Regex.IsMatch(phoneBookPhoneNumber, regex_periods) &&
                !Regex.IsMatch(phoneBookPhoneNumber, regex_one_space) && !Regex.IsMatch(phoneBookPhoneNumber, regex_one_period) &&
                !Regex.IsMatch(phoneBookPhoneNumber, regex_threes) && !Regex.IsMatch(phoneBookPhoneNumber, regex_plus_one_brackets) &&
                !Regex.IsMatch(phoneBookPhoneNumber, regex_fives) && !Regex.IsMatch(phoneBookPhoneNumber, regex_three_one) &&
                !Regex.IsMatch(phoneBookPhoneNumber, regex_long_country) && !Regex.IsMatch(phoneBookPhoneNumber, regex_two_four) && 
                !Regex.IsMatch(phoneBookPhoneNumber, regex_four_two)) && phoneBookPhoneNumber.Length > 8)
            {
                throw new ArgumentException("Does not match any of the accepted formats, please try again.");
            }

            Log($"{DateTime.Now}: Added phone book entry for {phoneBookEntry.Name}.");
            _phoneBookEntries.Add(phoneBookEntry.Name, phoneBookEntry.PhoneNumber);
            SaveDataToCsv();
        }

        public void Add(string name, string phoneNumber)
        {
            if (name == null || phoneNumber == null)
            {
                throw new ArgumentException("Name and phone number must both be specified.");
            }

            _phoneBookEntries.Add(name, phoneNumber);
            SaveDataToCsv();
        }

        public IEnumerable<PhoneBookEntry> List()
        {
            List<PhoneBookEntry> entriesList = new List<PhoneBookEntry>();

            foreach (var name in _phoneBookEntries.Keys)
            {
                entriesList.Add(new PhoneBookEntry { Name = name, PhoneNumber = _phoneBookEntries[name] });
            }

            Log($"{DateTime.Now}: Listed the entries of the phonebook.");

            return entriesList;
        }

        public void DeleteByName(string name)
        {
            if (name.Contains(','))
            {
                string[] names = name.Split(',', StringSplitOptions.RemoveEmptyEntries);
                name = names[1].Trim() + " " + names[0].Trim();
            }

            if (!_phoneBookEntries.ContainsKey(name))
            {
                throw new NotFoundException($"No phonebook entry found containing name {name}.");
            }

            _phoneBookEntries.Remove(name);
            SaveDataToCsv();
            Log($"{DateTime.Now}: Deleted entry for {name}.");
        }

        public void DeleteByNumber(string number)
        {
            var name = _phoneBookEntries.Where(kvp => kvp.Value == number).FirstOrDefault().Key;
            if (name == null)
            {
                throw new NotFoundException($"No phonebook entry found containing phone number {number}.");
            }

            _phoneBookEntries.Remove(name);
            SaveDataToCsv();
            Log($"{DateTime.Now}: Deleted entry for {name}.");
        }

        private void LoadDataFromCsv()
        {
            if (!File.Exists(_csvFilePath))
            {
                return;
            }

            using (var reader = new StreamReader(_csvFilePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values.Length == 2)
                    {
                        _phoneBookEntries.Add(values[0], values[1]);
                    }
                }
            }
        }

        private void SaveDataToCsv()
        {
            using (var writer = new StreamWriter(_csvFilePath))
            {
                foreach (var entry in _phoneBookEntries)
                {
                    writer.WriteLine($"{entry.Key},{entry.Value}");
                }
            }
        }

        private void Log(string Log)
        {
            using (var writer = new StreamWriter(_auditLogFilePath, true))
            {
                writer.WriteLine(Log);
            }
        }
    }
}
