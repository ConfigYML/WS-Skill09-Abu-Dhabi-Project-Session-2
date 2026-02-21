using Microsoft.EntityFrameworkCore;

namespace Session_2_Dennis_Hilfinger;

public partial class ImportPage : ContentPage
{
	int correctCount;
	int duplicateCount;
	int incompleteCount;
	public ImportPage()
	{
		InitializeComponent();
	}

	private async void SelectFile(object sender, EventArgs e)
	{
		var result = await FilePicker.PickAsync(new PickOptions()
		{
			PickerTitle = "Select a csv file to import",
		});
		if(result != null)
		{
			if (!result.FileName.EndsWith(".csv"))
			{
				await DisplayAlert("Error", "Please select a csv file", "OK");
				return;
            }
            PathEntry.Text = result.FullPath;
		}
    }

	private async void ImportFile(object sender, EventArgs e)
	{
		if(string.IsNullOrEmpty(PathEntry.Text) || !Path.Exists(PathEntry.Text) || !PathEntry.Text.EndsWith(".csv"))
		{
			await DisplayAlert("Error", "Please select a valid file path to import", "OK");
			return;
		}

		correctCount = 0;
		duplicateCount = 0;
		incompleteCount = 0;

		var lines = await File.ReadAllLinesAsync(PathEntry.Text);
		using (var db = new AirlineContext())
		{
			foreach (var line in lines)
			{
				var values = line.Split(',');
				if (values[0] == "ADD")
				{
					try
					{

						DateOnly flightDate = DateOnly.Parse(values[1]);
						TimeOnly flightTime = TimeOnly.Parse(values[2]);
						string flightNumber = values[3];
						string departureIata = values[4];
						string arrivalIata = values[5];
						int aircraft = int.Parse(values[6]);
						decimal basePrice = decimal.Parse(values[7]);
						bool confirmed;
						if (values[8] == "OK")
						{
							confirmed = true;
						}
						else if (values[8] == "CANCELLED")
						{
							confirmed = false;
						}
						else
						{
							throw new Exception();
						}

						var departureAirport = db.Airports.FirstOrDefault(a => a.Iatacode == departureIata);
						var arrivalAirport = db.Airports.FirstOrDefault(a => a.Iatacode == arrivalIata);

						if (departureAirport == null || arrivalAirport == null)
						{
							throw new Exception();
						}

						Schedule sched = new Schedule
						{
							Date = flightDate,
							Time = flightTime,
							FlightNumber = flightNumber,
							AircraftId = aircraft,
							EconomyPrice = basePrice,
							Confirmed = confirmed
						};


						if (db.Schedules.Any(s => s.Date == sched.Date && s.FlightNumber == sched.FlightNumber))
						{
							duplicateCount++;
							DisplayAlert("Duplicate entry", $"A schedule with the flight number {sched.FlightNumber} on the date {sched.Date} already exists in the database. This entry will be skipped.", "OK");
                        }
						else
						{
							var existantRoute = db.Routes.FirstOrDefault(r => r.ArrivalAirportId == arrivalAirport.Id && r.DepartureAirportId == departureAirport.Id);
							if (existantRoute == null)
							{
								int highestRouteId = db.Routes.Max(r => r.Id);
								Route route = new Route
								{
									Id = highestRouteId + 1,
									ArrivalAirportId = arrivalAirport.Id,
									DepartureAirportId = departureAirport.Id
								};
								db.Routes.Add(route);
								await db.SaveChangesAsync();
								sched.RouteId = route.Id;

							}
							else
							{
								sched.RouteId = existantRoute.Id;
							}

							db.Schedules.Add(sched);
							await db.SaveChangesAsync();
							correctCount++;
						}

					}
					catch
					{
						incompleteCount++;
					}

				}
				else if (values[0] == "EDIT")
				{
					var scheduleToEdit = db.Schedules
						.Include(s => s.Route)
                        .FirstOrDefault(s => s.FlightNumber == values[3] && s.Date == DateOnly.Parse(values[1]));
					if (scheduleToEdit != null)
					{
						try
						{
							scheduleToEdit.Date = DateOnly.Parse(values[1]);
                            scheduleToEdit.Time = TimeOnly.Parse(values[2]);
							scheduleToEdit.FlightNumber = values[3];
                            scheduleToEdit.AircraftId = int.Parse(values[6]);
							scheduleToEdit.EconomyPrice = decimal.Parse(values[7]);
							if (values[8] == "OK")
							{
								scheduleToEdit.Confirmed = true;
							}
							else if (values[8] == "CANCELLED")
							{
								scheduleToEdit.Confirmed = false;
							}
							else
							{
								throw new Exception();
							}

							var route = scheduleToEdit.Route;
                            var departureAirport = db.Airports.FirstOrDefault(a => a.Iatacode == values[4]);
							var arrivalAirport = db.Airports.FirstOrDefault(a => a.Iatacode == values[5]);

                            if (route == null || departureAirport == null || arrivalAirport == null)
                            {
                                throw new Exception();
                            }
                            route.DepartureAirportId = departureAirport.Id;
							route.ArrivalAirportId = arrivalAirport.Id;

							db.Routes.Update(route);
                            db.Schedules.Update(scheduleToEdit);
							await db.SaveChangesAsync();
							correctCount++;
						}
						catch
						{
							incompleteCount++;
						}
					}
					else
					{
						incompleteCount++;
                    }
                }
				else
				{
					incompleteCount++;
				}
			}
		}

		SuccessfulChangesLabel.Text = correctCount.ToString();
		DuplicateValuesLabel.Text = duplicateCount.ToString();
		IncompleteEntriesLabel.Text = incompleteCount.ToString();

    }

	private async void Cancel(object sender, EventArgs e)
	{
		ShellNavigationQueryParameters parameters = new ShellNavigationQueryParameters()
		{
			{ "Testing", "test" }
		};
        await Shell.Current.GoToAsync("..", parameters);
    }
}