const path = require('path');
const TerserPlugin = require('terser-webpack-plugin');
const { VueLoaderPlugin } = require("vue-loader");

module.exports = function (env, argv) {
    console.log(env, argv)
    console.log(`watch: ${argv.watch}`);
    console.log(`mode: ${argv.mode}`);
    return {
        mode: argv.mode,
        devtool: argv.mode == 'production' ? 'source-map' : 'eval',
        entry: './src/app.js',
        output: {
            path: argv.watch ? path.resolve(__dirname, '../bin/Debug/netcoreapp3.1/wwwroot')
                             : path.resolve(__dirname, '../wwwroot'),
            filename: "bundle.js"
        },
        module: {
            rules: [
                {
                    test: /\.css$/,
                    use: ["vue-style-loader", "css-loader"]
                },
                {
                    test: /\.vue$/,
                    loader: "vue-loader"
                },
                {
                    test: /\.(woff|woff2|eot|ttf|svg)$/,
                    use: [
                        {
                            loader: 'file-loader',
                            options: {
                                name: '[name].[ext]',
                                outputPath: 'fonts/'
                            }
                        }
                    ]
                }
            ]
        },
        plugins: [
            new VueLoaderPlugin()
        ],
        optimization: {
            minimizer: false
                ? []
                : [
                    new TerserPlugin({
                        extractComments: 'all',
                        terserOptions: {
                            compress: { drop_console: true }
                        },
                    }),
                ]
        }
    };
}