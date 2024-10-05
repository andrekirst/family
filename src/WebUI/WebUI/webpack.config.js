const path = require('path');
const glob = require('glob');

function getEntryPoints() {
    const entries = {};

    const files = glob.sync(path.resolve(__dirname, 'Areas/**/Views/**/*.cshtml.ts'));

    files.forEach(file => {
        const relativePath = path.relative(path.resolve(__dirname, 'Areas'), file);

        let outputName = relativePath
            .replace(/\/Views\//, '-')
            .replace(/\//g, '-')
            .replace('.cshtml.ts', '');

        outputName = outputName.toLowerCase();
        
        entries[outputName] = file;
        console.table(entries);
    });

    return entries;
}

module.exports = {
    entry: getEntryPoints(),
    output: {
        filename: '[name].js',
        path: path.resolve(__dirname, 'wwwroot/js')
    },
    module: {
        rules: [
            {
                test: /\.ts$/,
                use: 'ts-loader',
                exclude: /node_modules/
            }
        ]
    },
    resolve: {
        extensions: [
            '.ts',
            '.js'
        ]
    },
    mode: 'development'
}