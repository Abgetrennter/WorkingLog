const sharp = require('sharp');
const pngToIco = require('png-to-ico');
const fs = require('fs');
const path = require('path');

async function convert() {
    try {
        const svgPath = path.join(__dirname, '../logo.svg');
        const tempPngPath = path.join(__dirname, 'temp.png');
        const icoPath = path.join(__dirname, '../WorkLogApp.UI/app.ico');

        console.log('Converting SVG to PNG...');
        await sharp(svgPath)
            .resize(256, 256)
            .png()
            .toFile(tempPngPath);

        console.log('Converting PNG to ICO...');
        const buf = await pngToIco.default([tempPngPath]);
        fs.writeFileSync(icoPath, buf);

        console.log('Cleaning up...');
        fs.unlinkSync(tempPngPath);

        console.log('Success! Icon saved to ' + icoPath);
    } catch (error) {
        console.error('Error converting icon:', error);
        process.exit(1);
    }
}

convert();
