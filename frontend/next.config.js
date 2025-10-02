/** @type {import('next').NextConfig} */
const nextConfig = {
    // Configuração do Proxy Reverso para ambiente de desenvolvimento
    async rewrites() {
        return [
            {
                // Qualquer requisição começando com /api/ será redirecionada
                source: '/api/:path*',
                // O destino é o Backend .NET (porta 5196)
                destination: 'http://localhost:5196/:path*', 
            },
        ];
    }
};

module.exports = nextConfig;