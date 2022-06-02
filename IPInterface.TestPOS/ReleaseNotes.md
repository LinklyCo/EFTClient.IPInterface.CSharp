# Release Notes

## v1.11.1 22/04/2022 Robert Whitlock
- [STRY0182716] Built with IPInterface v1.6.7

## v1.11.0 08/04/2022 Daniel Parubotchy
- [MID-213] Updated to use updated version of the IPInterface library (1.6.6.0)

## v1.10.0 15/03/2022 Robert Whitlock
- [VA-509] Adding extra slave support to handle entry/exit from slave mode and keeping connection open when in Woolies mode
- [MID-268] Built with new version of IPInterface library, adding more EFTTerminalTypes (returned in `EFTStatusResponse`)

## v1.9.0.0 10/03/2022 Robert Whitlock
- Adding Raw Data functionality to TestPOS

## v1.8.0.0 04/03/2022 Aaron Zimmermann
- Fixed issue where Test POS becomes unresponsive after receiving unexpected message

## v1.7.9.0 24/02/2022 Robert Whitlock
- Rebuilding with new version of IPInterface (v1.6.4)

## v1.7.8.0 25/01/2021 Robert Whitlock
- Adding check before executing EFT command on button press
- Simplifying main form xaml (no visible change)

## v1.7.7.0 16/12/2021 Aaron Zimmermann
- Updated IPInterface

## v1.7.6.0 29/07/2021 RJN
- Fix: TestPOS crashes when pairing fails authentication.

## v1.7.5.0 26/07/2021 Robert Whitlock

- Removing 5 minute message timeout for woolies
- Displays QR code from Display Event PAD data:
  - Loads from local file DAT.PTH if DAT.TAG == "QRI"
  - If the above isn't provided, Generates QR code from QRC content
- Adding '8' Barcode Only option as Display Input Type to match what woolies is currently using for giftcards
- Allowing any message to be sent if one is already is in progress if in woolies mode
- Making options UI more user friendly
- Adding option of query card type for Query + Transaction
- Resetting Pan Source after Query + Transaction completes

## v1.7.4.2 16/04/2021 Robert Whitlock

- SendKey with data will not cancel an in-progress Query Card when in Woolies POS mode

## v1.7.4.1 20/01/2021 Recel Jimenez

- Fix SendKey not connecting to selected endpoint
- Allow 'B' or 'K' at the start of the input data to be treated as Barcode and Keyed entry types respectively

## v1.7.3.0 20/01/2021 Robert Whitlock

- Enabling 'B' (Barcode) and 'K' (Keyboard) as send key 'Key' values when in Woolies POS mode
- Doing a status will populate the Merchant Config details (if response is valid)
- Auto Transaction Reference is 8 digits when in Woolies POS mode (HHmmssff)
- Fixes a crash when doing a send key with IS_WOW client config unset
- Query + Transaction button now sends J8 as query part when in Woolies POS mode
    - PAD field from Query response will be used in the Transaction request

## v1.7.2.0 16/12/2020 Robert Whitlock
- #2034 Reworking how field save/restore works to have less impact on behaviour
- #2032 Saving Merchant Config values as well
- #2031 Removed trimming of '2' character from display prompts
- #2030 Send Key abandons in progress Query Card in Woolies POS mode
- Enter key can be used to send key from display prompt entry boxes

## v1.7.1.0 1/12/2020 Robert Whitlock
- Transaction/Logon/Query Card field values are now saved on application exit (and restored on startup)
- Added a button to reset the saved field values

## v1.7.0.0 13/11/2020 Robert Whitlock
- Adding Auto-Connect mode to WoolworthsPOS mode

## v1.6.2.2 5/11/2020 Robert Whitlock
- Adding Save/Clear buttons to Log tab
- Fixing issue where an existing endpoint wouldn't be saved it it had been modified
- Adding an unpair button to Endpoint editor
- Adding apply/cancel/ok buttons to endpoint editor
- Fixing message name in '<x> in progress' button
- Adding loading cursor after clicking pair pinpad
- History now taking up the entire panel

## v1.6.2.1 5/10/2020 Robert Whitlock
- Re-adding scrollbar to Response tab

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
