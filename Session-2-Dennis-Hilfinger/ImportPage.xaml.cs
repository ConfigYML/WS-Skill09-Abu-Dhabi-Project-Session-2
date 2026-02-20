namespace Session_2_Dennis_Hilfinger;

public partial class ImportPage : ContentPage
{
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

		var lines = await File.ReadAllLinesAsync(PathEntry.Text);
		foreach( var line in lines)
		{
			var values = line.Split(',');
			if (values[0] == "ADD")
			{
				try
				{
                    using (var db = new AirlineContext())
                    {
                        DateOnly flightDate = DateOnly.Parse(values[1]);
                        TimeOnly flightTime = TimeOnly.Parse(values[2]);
                        string flightNumber = values[3];
						db.Schedules.Add(new Schedule
						{
							Date = flightDate,
							Time = flightTime,
							FlightNumber = flightNumber

						});
                    }
                } catch
				{
					await DisplayAlert("Error", $"Error in line: \n{line}\nPlease review.", "Ok");
				}
				
			} else if (values[0] == "EDIT")
			{
				// TODO: implement edit logic
			}
		}

    }
}