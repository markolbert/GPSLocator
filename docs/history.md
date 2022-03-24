# History

**Navigation**: Home -> History

This displays the history of tracking points:

![history initial view](assets/history-initial.png)

It may take a few seconds to populate because data has to be retrieved from servers.

The **End Date** and **Start Date** fields will update, once a second, to show you the timespan that was retrieved or will be retrieved when you click the **Refresh** button.

By default the history page retrieves everything from the past week. That time frame can be increased by adjusting the Days Back setting. After changing Days Back click the Refresh button to apply the new time frame.

You can also limit what's retrieved to only those tracking points associated with messages sent from the device (all InReach messages are automatically associated with a tracking point whether or not you have tracking enabled on the InReach device).

Checking the box will delete any previously-retrieved points that do **not** have messages associated with them. However, **unchecking** the box will not automatically retrieve "unmessaged" points. You have to click the Refresh button to do that.

## Selecting points

Once points have been retrieved you can see their details by clicking on them:

![one point of history](assets/history-onept.png)

When selected, a point's details will appear and a map showing the location of the tracking point will display. You may need to expand the window to see all the details and/or the map.

There is one case where the map won't be displayed after clicking a point: some InReach points appear to come from latitude/longitude 0/0. These are ignored by the map display.

The map is pannable (by clicking and dragging it) and zoomable (by using your mouse wheel) You can also zoom it by using the + / - buttons.

## Points with messages

If a selected point has a message information about the message will appear in the detail view:

![a point with a message](assets/history-message.png)

The recipient(s) were hidden in the image for privacy reasons. They show up in the app.
