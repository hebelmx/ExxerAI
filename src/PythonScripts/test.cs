 private void SetupSignalREvents()
 {
     _hubConnection.On<int, PlcMonitor>("ReceiveHeartBeat", (plcId, plcMonitor) =>
     {
         if (Stations.ContainsKey(plcId))
         {
             var station = Stations[plcId];
             station.PartNumber = plcMonitor.PartNumber;
             station.StatusColor = plcMonitor.HeartBeat > 0 ? "success" : "error"; // Update color based on heartbeat
             station.Parameters["StatusMessage"] = plcMonitor.HeartBeat > 0 ? "Active" : "No Heartbeat";
             station.Parameters["Step"] = plcMonitor.HeartBeat > 0 ? "2" : "1"; // Example update
         }

         InvokeAsync(StateHasChanged);
     });

     _hubConnection.On<int, ProcessMonitor>("ReceiveProcessMonitor", (plcId, processMonitor) =>
     {
         if (Stations.ContainsKey(plcId))
         {
             var station = Stations[plcId];
             station.Parameters["StatusMessage"] = processMonitor.Message;
             station.Parameters["Step"] = processMonitor.Step.ToString();
             station.StatusColor = processMonitor.IsScrap ? "error" : "success"; // Example logic for scrap
         }

         InvokeAsync(StateHasChanged);
     });
 }