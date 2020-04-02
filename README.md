## Sitecore Image Compression (Helix Format) 
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

## What does this resolve in regard to Page Speed ?

This module addresses the image optimisation component of page speed. The aim is to optimise your images to sizes that google will appreciate. Googlie also now recommends using a next-gen image format such as webp, this module also allows you to convert your existing images to that format and to meet the Google Page Speed recommendation. 

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

## References and Inspiration

* [Recommend this is used alongside Sitecore Speedy for SXA](https://github.com/Aceik/Sitecore-Speedy)
* [Tiny PNG](https://tinypng.com/)
* [Tiny PNG API](https://tinypng.com/developers)
* [Google Webp format](https://developers.google.com/speed/webp)
* [Kraken.io](https://kraken.io/docs/getting-started)


## Other Implementation of Sitecore and Tiny PNG
* https://jockstothecore.com/optimizing-images-sitecore/  (Does the crunch via powershell)
* https://sitecoremaster.github.io/Sitecore-Media-Library-Enhancements/   (Does the crunch on upload)
