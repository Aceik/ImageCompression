## Sitecore Image Compression (Helix Format) 
<img src='https://img.shields.io/github/tag/Aceik/ImageCompression.svg' />
<img src='https://img.shields.io/github/issues/Aceik/ImageCompression.svg' />
<img src='https://img.shields.io/github/license/Aceik/ImageCompression.svg' />
<img src='https://img.shields.io/github/languages/code-size/Aceik/ImageCompression.svg' />

Compress your Sitecore library with the Tiny PNG API. Crunch images to improve you page speed. 

## What does it do ?

Connect you media library to the Tiny PNG API and optimise your images in the background. 
Introduces a button for each image in the Sitecore tab to do this as you go or performs the compression of all images via a scheduled task. 
Shows the before and after results in a Image meta data field, so that you can tell the difference and also know which images have been processed. 

## What does this resolve in regard to Page Speed ?

This module addresses the image optimisation component of page speed. The aim is to compress your images to sizes that google will appreciate.

## Is it easy to use ?

Yes, once installed you can leave the scehduled task running in the background and your Sitecore Media Library will be compressed via the Tiny PNG API automatically. 

## Installation prerequisites and notes

1)  <img src="https://img.shields.io/badge/requires-sitecore-blue.svg?style=flat-square" alt="requires sitecore">
  * <img src="https://img.shields.io/badge/supports-sitecore%20v9.3-green.svg?style=flat-square" alt="requires sitecore 9.3">
  * <img src="https://img.shields.io/badge/supports-helix-green.svg?style=flat-square" alt="requires Helix Foundation"/>

## Getting Started Steps
1) Installation
- Option 1: [via Sitecore Package](https://github.com/Aceik/Sitecore-Speedy/wiki/00-Installation-Via-Sitecore-Package)
- Option 2: [Via Source](https://github.com/Aceik/Sitecore-Speedy/wiki/01--Installation-Via-Helix-Source)

### Sitecore Settings
* [Global Settings](https://github.com/Aceik/Sitecore-Speedy/wiki/06---Global-Settings)

## References and Inspiration

* [Recommend this is used alongside Sitecore Speedy for SXA](https://github.com/Aceik/Sitecore-Speedy)
* [Tiny PNG](https://tinypng.com/)
* [Tiny PNG API](https://tinypng.com/developers)


## Other Implementation of Sitecore and Tiny PNG
* https://jockstothecore.com/optimizing-images-sitecore/  (Does the crunch via powershell)
* https://sitecoremaster.github.io/Sitecore-Media-Library-Enhancements/   (Does the crunch on upload)
