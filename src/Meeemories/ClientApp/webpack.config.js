const path = require('path');
const TerserPlugin = require('terser-webpack-plugin');
const { VueLoaderPlugin } = require("vue-loader");

module.exports = function (env, argv) {
    return {
        mode: env.production ? 'production' : 'development',
        devtool: env.production ? 'source-map' : 'eval',
        entry: './src/app.js',
        output: {
            path: env.watch ? path.resolve(__dirname, '../wwwroot')
                            : path.resolve(__dirname, '../bin/Debug/netcoreapp3.1/wwwroot'),
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