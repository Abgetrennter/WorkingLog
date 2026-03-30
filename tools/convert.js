const sharp = require('sharp');
const pngToIco = require('png-to-ico');
const fs = require('fs');
const path = require('path');

async function convert() {
    try {
        const svgPath = path.join(__dirname, '../logo.svg');
        const icoPath = path.join(__dirname, '../WorkLogApp.UI/app.ico');

        // 需要生成的图标尺寸（正方形）
        const sizes = [16, 32, 48, 64, 128, 256];
        const tempPngPaths = [];

        console.log('Generating PNGs at multiple sizes...');
        for (const size of sizes) {
            const tempPath = path.join(__dirname, `temp_${size}.png`);
            await sharp(svgPath)
                .resize(size, size)
                .png()
                .toFile(tempPath);
            tempPngPaths.push(tempPath);
            console.log(`  ${size}x${size} generated`);
        }

        console.log('Converting PNGs to ICO...');
        const buf = await pngToIco.default(tempPngPaths);
        fs.writeFileSync(icoPath, buf);
        console.log('ICO saved to ' + icoPath);

        console.log('Cleaning up temporary files...');
        for (const tempPath of tempPngPaths) {
            fs.unlinkSync(tempPath);
        }
        console.log('Success!');
    } catch (error) {
        console.error('Error converting icon:', error);
        process.exit(1);
    }
}

convert();