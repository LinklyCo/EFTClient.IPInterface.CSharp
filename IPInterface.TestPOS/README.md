# TestPOS Custom Features

## QR Code Display

To test a Woolies feature to display a QR code in TestPOS, some special handling of PAD tag data has been implemented.

### Requirements:

* EFT-Client v4.1.18.87 or newer (adds PAD tag support to Pinpad Display Event interface)
* TestPOS v1.7.5 or newer

### PAD Data

In an `EFTDisplayResponse`, if the `PurchaseAnalysisData` contains the following tags a QR code should be displayed:

#### PAD data from file

`DATnnn`<span style="color:green">`TAG003QRI`</span><span style="color:magenta">`PTHmmm{filepath}`</span> (e.g. `DAT029TAG003QRIPTH014C:/temp/qr.png)`

Will display the QR image stored in `{filepath}`

#### PAD data from content

`QRCnnn{content}` (e.g. `QRC024http://www.linkly.com.au`)

The `{content}` data will be encoded into a QR image and displayed.

Please note: if both file and content QR data is contained in the PAD data, the file will be taken as preference.