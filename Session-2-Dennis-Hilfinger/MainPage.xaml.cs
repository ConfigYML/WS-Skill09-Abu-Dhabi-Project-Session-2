using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Collections.ObjectModel;
using Windows.Devices.AllJoyn;

namespace Session_2_Dennis_Hilfinger
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<FlightDTO> FlightList = new ObservableCollection<FlightDTO>();
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            FillFilterData();
            LoadData(null, null);
        }

        private async void FillFilterData()
        {
            SortingPicker.Items.Add("Date-Time");
            SortingPicker.Items.Add("Base price");
            SortingPicker.Items.Add("Confirmed");
            SortingPicker.SelectedIndex = 0;

            DeparturePicker.Items.Add("");
            DestinationPicker.Items.Add("");
            DeparturePicker.SelectedIndex = 0;
            DestinationPicker.SelectedIndex = 0;

            using(var db = new AirlineContext())
            {
                var airports = await db.Airports.ToListAsync();
                foreach (var airport in airports)
                {
                    DeparturePicker.Items.Add(airport.Iatacode);
                    DestinationPicker.Items.Add(airport.Iatacode);
                }
            }
        }


        private async void LoadData(object sender, EventArgs e)
        {
            using (var db = new AirlineContext())
            {
                string departure = DeparturePicker.SelectedItem.ToString();
                string destination = DestinationPicker.SelectedItem.ToString();
                DateOnly outboundDate;
                bool dateFilter = false;

                int flightNumber;
                bool hasFlightNumber = false;

                if (departure == destination && !String.IsNullOrEmpty(departure))
                {
                    await DisplayAlert("Info", "Departure and destination airport can not be the same. Please change your selection.", "Ok");
                    return;
                }

                /*var date = OutboundDateInput.Text.Split("/");
                if (date.Length != 3)
                {
                    await DisplayAlert("Info", "Please enter a valid date in the following format: dd/mm/yyyy", "Ok");
                    return;
                }
                int[] values = {int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0])};
                
                DateTime dt = new DateTime();*/

                if (!String.IsNullOrEmpty(OutboundDateInput.Text))
                {
                    if (!DateOnly.TryParse(OutboundDateInput.Text, out DateOnly outbound))
                    {
                        await DisplayAlert("Info", "Date was not in correct format.", "Ok");
                        return;
                    }
                    outboundDate = outbound;
                    dateFilter = true;

                }

                if (!String.IsNullOrEmpty(FlightNumberInput.Text))
                {
                    if (!int.TryParse(FlightNumberInput.Text, out int number))
                    {
                        await DisplayAlert("Info", "Flight number can only consist of numbers", "Ok");
                        return;
                    }
                    flightNumber = number;
                    hasFlightNumber = true;
                }

                FlightList.Clear();
                if (departure != null || destination != null || dateFilter || hasFlightNumber)
                {
                    var flights = db.Schedules
                        .Include(s => s.Route).ThenInclude(r => r.ArrivalAirport)
                        .Include(r => r.Route).ThenInclude(r => r.DepartureAirport);
                    IQueryable filtered = flights.Where(f => f.Id != null);
                    if (departure != null)
                    {
                        filtered = flights.Where(f => f.Route.DepartureAirport.Iatacode == departure)
                    }
                    
                } else
                {

                }
                FlightGrid.ItemsSource = FlightList;



            }
        }
        private async void FlightSelected(object sender, EventArgs e)
        {

        }
        private async void CancelFlight(object sender, EventArgs e)
        {

        }
        private async void EditFlight(object sender, EventArgs e)
        {

        }
        private async void ImportChanges(object sender, EventArgs e)
        {

        }



        /*private async void ImportData()
        {
            using (var db = new AirlineContext())
            {
                string[] lines = File.ReadAllLines("Data/UserData.csv");

                int userId = 0;
                if (db.Users.Count() != 0)
                {
                    userId = db.Users.Max(u => u.Id);
                }

                foreach(var line in lines)
                {
                    string[] data = line.Split(',');

                    string role = data[0];
                    string email = data[1];
                    string password = data[2];
                    string md5Password = GetMd5Password(password);

                    string firstname = data[3];
                    string lastname = data[4];
                    string office = data[5];

                    var birthDateParts = data[6].Split('/');
                    var year = int.Parse(birthDateParts[2]);
                    var month = int.Parse(birthDateParts[0]);
                    var day = int.Parse(birthDateParts[1]);
                    DateOnly birthdate = new DateOnly(year, month, day);

                    bool active = data[7] == "1" ? true : false;

                    var roleId = db.Roles.FirstOrDefault(r => r.Title.ToLower() == role.ToLower()).Id;
                    var officeId = db.Offices.FirstOrDefault(o => o.Title.ToLower() == office.ToLower()).Id;

                    if (db.Users.Any(u => u.Email.ToLower() == email.ToLower()))
                    {
                        continue;
                    };

                    User user = new User
                    {
                        Id = userId,
                        RoleId = roleId,
                        OfficeId = officeId,
                        Email = email,
                        Password = md5Password,
                        FirstName = firstname,
                        LastName = lastname,
                        Birthdate = birthdate,
                        Active = active
                    };
                    db.Users.Add(user);
                    userId++;
                }
                db.SaveChanges();
            }
        }*/

        public class FlightDTO
        {
            public DateOnly FlightDate { get; set; }
            public TimeOnly FlightTime { get; set; }
            public string DepartureAirport { get; set; }
            public string DestinationAirport { get; set; }
            public int FlightNumber { get; set; }
            public int Aircraft { get; set; }
            public int BasePrice { get; set; }
            public int BusinessPrice => int.Parse((BasePrice * 1.35).ToString());
            public int FirstClassPrice => int.Parse((BusinessPrice * 1.3).ToString());
            public bool Confirmed { get; set; }
        }

    }
    
}
