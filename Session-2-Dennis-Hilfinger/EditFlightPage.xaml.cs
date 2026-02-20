
using Microsoft.EntityFrameworkCore;

namespace Session_2_Dennis_Hilfinger;

public partial class EditFlightPage : ContentPage, IQueryAttributable
{
    int flightId = -1;
	public EditFlightPage()
	{
		InitializeComponent();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        flightId = int.Parse(query["flightId"].ToString());
        LoadData();
    }

    private async void LoadData()
    {
        using(var db = new AirlineContext())
        {
            var flight = db.Schedules
                .Include(f => f.Route).ThenInclude(r => r.DepartureAirport)
                .Include(f => f.Route).ThenInclude(r => r.ArrivalAirport)
                .FirstOrDefault(f => f.Id == flightId);
            DepartureLabel.Text = flight.Route.DepartureAirport.Iatacode;
            DestinationLabel.Text = flight.Route.ArrivalAirport.Iatacode;
            DatePicker.Date = flight.Date.ToDateTime(TimeOnly.MinValue);
            TimePicker.Time = flight.Time.ToTimeSpan();
            EconomyPriceEntry.Text = decimal.ToInt32(flight.EconomyPrice).ToString();
        }
    }

    private async void Save(object sender, EventArgs e)
    {
        using(var db = new AirlineContext())
        {
            var flight = db.Schedules.FirstOrDefault(f => f.Id == flightId);
            flight.Date = DateOnly.FromDateTime(DatePicker.Date);
            flight.Time = TimeOnly.FromTimeSpan(TimePicker.Time);
            flight.EconomyPrice = decimal.Parse(EconomyPriceEntry.Text);
            await db.SaveChangesAsync();
            ShellNavigationQueryParameters parameters = new ShellNavigationQueryParameters()
            {
                { "Test", "test" }
            };
            await Shell.Current.GoToAsync("..", parameters);
        }
    }

    private async void Cancel(object sender, EventArgs e)
    {
        ShellNavigationQueryParameters parameters = new ShellNavigationQueryParameters()
        {
            { "Test", "test" }
        };
        await Shell.Current.GoToAsync("..", parameters);
    }   
}