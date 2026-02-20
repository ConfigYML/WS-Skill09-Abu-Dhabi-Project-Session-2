using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Collections.ObjectModel;
using Windows.Devices.AllJoyn;

namespace Session_2_Dennis_Hilfinger
{
    public partial class MainPage : ContentPage, IQueryAttributable
    {
        public ObservableCollection<FlightDTO> FlightList = new ObservableCollection<FlightDTO>();
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            ApplyQueryAttributes(null);
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
                DateOnly outboundDate = DateOnly.MinValue;
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
                if (!String.IsNullOrEmpty(departure) || !String.IsNullOrEmpty(destination) || dateFilter || hasFlightNumber)
                {
                    var flights = db.Schedules
                        .Include(s => s.Route).ThenInclude(r => r.ArrivalAirport)
                        .Include(r => r.Route).ThenInclude(r => r.DepartureAirport);
                    IQueryable<Schedule> filtered = flights.Where(f => f.Id != null);

                    if (!String.IsNullOrEmpty(departure))
                    {
                        filtered = filtered.Where(f => f.Route.DepartureAirport.Iatacode == departure);
                    }
                    if (!String.IsNullOrEmpty(destination))
                    {
                        filtered = filtered.Where(f => f.Route.ArrivalAirport.Iatacode == destination);
                    }
                    if (dateFilter)
                    {
                        filtered = filtered.Where(f => f.Date == outboundDate);
                    }
                    if (hasFlightNumber)
                    {
                        filtered = filtered.Where(f => f.FlightNumber.Contains(FlightNumberInput.Text));
                    }

                    foreach (var flight in filtered)
                    {
                        FlightList.Add(new FlightDTO
                        {
                            Id = flight.Id,
                            FlightDate = flight.Date,
                            FlightTime = flight.Time,
                            DepartureAirport = flight.Route.DepartureAirport.Iatacode,
                            DestinationAirport = flight.Route.ArrivalAirport.Iatacode,
                            FlightNumber = int.Parse(flight.FlightNumber),
                            Aircraft = flight.AircraftId,
                            BasePrice = decimal.ToInt32(flight.EconomyPrice),
                            Confirmed = flight.Confirmed
                        });
                    }
                } else
                {
                    var flights = db.Schedules
                        .Include(s => s.Route).ThenInclude(r => r.ArrivalAirport)
                        .Include(r => r.Route).ThenInclude(r => r.DepartureAirport);
                    foreach (var flight in flights)
                    {
                        FlightList.Add(new FlightDTO
                        {
                            Id = flight.Id,
                            FlightDate = flight.Date,
                            FlightTime = flight.Time,
                            DepartureAirport = flight.Route.DepartureAirport.Iatacode,
                            DestinationAirport = flight.Route.ArrivalAirport.Iatacode,
                            FlightNumber = int.Parse(flight.FlightNumber),
                            Aircraft = flight.AircraftId,
                            BasePrice = decimal.ToInt32(flight.EconomyPrice),
                            Confirmed = flight.Confirmed
                        });
                    }
                }

                string sorting = SortingPicker.SelectedItem.ToString();
                switch (sorting)
                {
                    case "Date-Time":
                        var dateSortedFlights = FlightList.OrderByDescending(f => f.FlightDt);
                        FlightGrid.ItemsSource = dateSortedFlights;
                        break;
                    case "Base price":
                        var priceSortedFlights = FlightList.OrderByDescending(f => f.BasePrice);
                        FlightGrid.ItemsSource = priceSortedFlights;
                        break;
                    case "Confirmed":
                        var confirmedSortedFlights = FlightList.OrderByDescending(f => f.Confirmed);
                        FlightGrid.ItemsSource = confirmedSortedFlights;
                        break;
                }
                

            }
        }
        private async void FlightSelected(object sender, EventArgs e)
        {
            if (FlightGrid.SelectedItem != null)
            {
                EditBtn.IsEnabled = true;
                CancelBtn.IsEnabled = true;
                using(var db = new AirlineContext())
                {
                    FlightDTO flight = FlightGrid.SelectedItem as FlightDTO;
                    var fl = await db.Schedules.FirstOrDefaultAsync(s => s.Id == flight.Id);
                    if (fl.Confirmed)
                    {
                        CancelBtn.Text = "Cancel flight";
                    } else
                    {
                        CancelBtn.Text = "Confirm flight";
                    }
                }
            } else
            {
                EditBtn.IsEnabled = false;
                CancelBtn.IsEnabled = false;
                CancelBtn.Text = "Cancel flight";
            }
        }
        private async void CancelFlight(object sender, EventArgs e)
        {
            if (FlightGrid.SelectedItem != null)
            {
                FlightDTO flight = FlightGrid.SelectedItem as FlightDTO; 
                using (var db = new AirlineContext())
                {
                    var fl = db.Schedules.FirstOrDefault(s => s.Id == flight.Id);
                    fl.Confirmed = !fl.Confirmed;
                    db.Update(fl);
                    await db.SaveChangesAsync();
                    LoadData(null, null);
                }
            }
        }

        private async void EditFlight(object sender, EventArgs e)
        {
            if (FlightGrid.SelectedItem != null)
            {
                FlightDTO flight = FlightGrid.SelectedItem as FlightDTO;
                using (var db = new AirlineContext())
                {
                    ShellNavigationQueryParameters parameters = new ShellNavigationQueryParameters()
                    {
                        { "flightId", flight.Id }
                    };
                    await Shell.Current.GoToAsync("EditFlightPage", parameters);
                }
            }
        }

        private async void ImportChanges(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ImportPage");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            FillFilterData();
            LoadData(null, null);
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
            public int Id { get; set; }
            public DateOnly FlightDate { get; set; }
            public TimeOnly FlightTime { get; set; }
            public DateTime FlightDt => new DateTime(FlightDate.Year, FlightDate.Month, FlightDate.Day, FlightTime.Hour, FlightTime.Minute, FlightTime.Second);
            public string DepartureAirport { get; set; }
            public string DestinationAirport { get; set; }
            public int FlightNumber { get; set; }
            public int Aircraft { get; set; }
            public int BasePrice { get; set; }
            public int BusinessPrice => (int)(BasePrice * 1.35);
            public int FirstClassPrice => (int)(BusinessPrice * 1.3);
            public bool Confirmed { get; set; }
        }

    }
    
}
