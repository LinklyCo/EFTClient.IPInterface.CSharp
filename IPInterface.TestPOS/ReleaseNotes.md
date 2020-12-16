# Release Notes

## v1.6.2.0 30/09/2020 Robert Whitlock
- Adding TestPOS application icon
- Adding version to title bar
- Example PAD tag is saved/viewable in editor
- Entered Track2 values get saved when present in a transaction request
- Track2 Collection Editor no longer has PAD specific text/function
- PAD tags that are manually entered are saved for future use
- Added tooltips to Append PAD tag checkboxes
- Added checkbox to set POS barcode enabled in the PCM tag
- Result tab now shows what the response type was
- Display response data now shows in the response tab
- Preventing saved value for the Response Dialog from being overwritten when changing endpoint
- Adding Profiler tab which keeps a history of messages and their timestamps
- Adding function to move items in the Pad editor
- Unknown cloud commands are now saved (to slave.json)
- Entered cloud commands are now editable (a la PAD/Track2)
- Spaces are now visible in cloud command entry (and collection editor data)
- Adding Buttons for common Slave mode commands
- Shifting slave mode from Other to the main tabs

## v1.6.0.1 28/09/2020 Clinton Dean
- Configure default basket, amount and currency code

## v1.6.0.0 14/09/2020 Clinton Dean
- Cloud 2 Support

## v1.5.0.0 21/08/2020 Robert Whitlock
- Allows status/transaction messages to be sent if a query card is active (if 'Woolworths POS' is checked)
- Log panel auto-scrolls to the bottom
- Shows which request is in progress and gives the ability to cancel it
- Added new Transaction Type 'G' for MOTO
- PAD data at the end of the Query Card handles both when there is a prefixed length present or not (customer implemented this out-of-spec)
