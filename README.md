## Sitecore Image Compression and Next-Gen Conversion (Helix Format) 
<img src='https://img.shields.io/github/tag/Aceik/ImageCompression.svg' />
<img src='https://img.shields.io/github/issues/Aceik/ImageCompression.svg' />
<img src='https://img.shields.io/github/license/Aceik/ImageCompression.svg' />
<img src='https://img.shields.io/github/languages/code-size/Aceik/ImageCompression.svg' />


Compress and Convert the images in your Sitecore media library with the Tiny PNG and Kraken.io. 
Crunch images to improve you page load times and image payload sizes. 
Convert images to webp for next generation image format improvements. 

## What does it do ?

This module will connect your media library to imaging APIs that optimise and convert your images. 
Introduces a button for each image in the Sitecore tab to do this as you go or works via via a scheduled task that checks all images. 
Shows the before and after results in a Image meta data field, so that you can tell the difference and also know which images have been processed. 

<img src='https://aceiksolutions.files.wordpress.com/2020/04/image-10.png?w=1024' />

## What does this resolve in regard to Page Speed ?

This module addresses the image optimisation component of page speed. The aim is to optimise your images to sizes that google will appreciate. Google also now recommends using a next-gen image format such as webp, this module also allows you to convert your existing images to that format and to meet the Google Page Speed recommendation. 

## Is it easy to use ?

Yes, once installed you can leave the scehduled task running in the background and your Sitecore Media Library will be optimised/converted via the API integrations automatically. 

## What does it cost ?
Depending on your usage you may be able to remain within the free tier of both API's used. 
Have a look at Tiny PNG and Kraken.io respectively for this information. 
You can also opt to turn one of that API connections off if you don't want to use it.

## Installation prerequisites and notes

1)  <img src="https://img.shields.io/badge/requires-sitecore-blue.svg?style=flat-square" alt="requires sitecore">
  * <img src="https://img.shields.io/badge/supports-sitecore%20v9.3-green.svg?style=flat-square" alt="requires sitecore 9.3">
  * <img src="https://img.shields.io/badge/supports-helix-green.svg?style=flat-square" alt="requires Helix Foundation"/>

## Getting Started Steps
1) Installation
- Option 1: via Sitecore Package -- Look for the installable package in the releases of this repo.
- Option 2: Via Source

2) Media Library Template Inheritance
- This module adds a new field to your image template. This requires us to inherit from a newly added template that the module installs. 
- Locate the Image template inside Sitecore /sitecore/templates/System/Media/Unversioned/Image
- Make it inherit the following template  /sitecore/templates/Foundation/ImageCompression/RelatedCompressedImage
- Now check an existing image in the Media Library.  A new field should be added under the Media tab called "Related Compressed File".

3) Update API Settings and Web.config
- Choose either to use Tiny PNG or Kraken.IO    (you can use another service but you may need to cut some code and contribute)
- Adjust settings inside  /sitecore/system/Settings/Foundation/ImageCompression/Image Compression Settings
    * Tiny PNG settings are availabe under the section "Image Compression Endpoint"
      - Enter your Tiny PNG Key into the field "Compression End Point Key"
      - Check the Tiny PNG enpoint field is correct (this should come from standard values)
      - The field "Compression End Point Secret" is not required for Tiny PNG
      - See standard settings inside <a href='https://aceiksolutions.files.wordpress.com/2020/04/compression.png?w=500'>screenshot for compression.</a>
    * Kraken.IO settings are availabe under the section "Image Conversion Endpoint"
      - Enter your Krarken.IO API KEY and secret into "Convert End Point Key" and "Convert Endpoint Secret"
      - See standard settings inside <a href='https://aceiksolutions.files.wordpress.com/2020/04/conversion.png?w=500'>screenshot for conversion.</a>
	  - For WEBP Support we need to replace the default Sitecore image handler. This madness is simply due to Safari and IE not supporting webp.
		- To achieve this we simply sniff out the Browser Header in the code and if its Safari or IE we send back the original JPEG. 
		- IF the browser is Chrome or Firefox we can send back the WEBP images. Which have a lot better compression.
		- A) Locate this entry in web.conf -> `<add verb="*" path="sitecore_media.ashx" type="Sitecore.Resources.Media.MediaRequestHandler, Sitecore.Kernel" name="Sitecore.MediaRequestHandler" />`
		- B) For Non SXA installations replace with -> `<add verb="*" path="sitecore_media.ashx" type="Sitecore.Foundation.ImageCompression.MediaRequestHandler, Sitecore.Foundation.ImageCompression" name="Sitecore.MediaRequestHandler" />`
		- B) For SXA installations replace with -> `<add verb="*" path="sitecore_media.ashx" type="Sitecore.Foundation.ImageCompression.MediaRequestHandlerXA, Sitecore.Foundation.ImageCompression" name="Sitecore.MediaRequestHandler" />`
- Adjust the other settings as needed. You can:
    * turn off either of the above API services
    * disable the scheduled tasks for either
    * disable either of the buttons in the media panel

## References and Inspiration

* [Recommend this is used alongside Sitecore Speedy for SXA](https://github.com/Aceik/Sitecore-Speedy)
* [Tiny PNG](https://tinypng.com/)
* [Tiny PNG API](https://tinypng.com/developers)
* [Google Webp format](https://developers.google.com/speed/webp)
* [Kraken.io](https://kraken.io/docs/getting-started)


## Other Implementation of Sitecore and Tiny PNG
* https://jockstothecore.com/optimizing-images-sitecore/  (Does the crunch via powershell)
* https://sitecoremaster.github.io/Sitecore-Media-Library-Enhancements/   (Does the crunch on upload)
