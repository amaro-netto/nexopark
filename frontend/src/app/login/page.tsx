'use client';
import { useState } from 'react';
import axios from 'axios';

// Configuração Base: Use o endereço da sua API
const API_BASE_URL = 'http://localhost:5196'; 

export default function LoginPage() {
  // Credenciais padrões para facilitar o teste local
  const [email, setEmail] = useState('admin@nexopark.com'); 
  const [senha, setSenha] = useState('admin123');
  const [message, setMessage] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('Tentando logar...');

    try {
      // 1. Faz a requisição de POST para o endpoint de login da API
      const response = await axios.post(`${API_BASE_URL}/login`, { email, senha });
      
      const token = response.data.token;
      
      // 2. ARMAZENAMENTO TEMPORÁRIO PARA DEBUG: 
      // Em produção, isso seria perigoso (XSS) e deveria ser HttpOnly Cookie.
      localStorage.setItem('userToken', token);
      
      setMessage(`Login bem-sucedido! Token armazenado para testes. Redirecionando...`);

      // 3. Redireciona para a página de veículos
      window.location.href = '/veiculos';

    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        if (error.response.status === 401) {
             setMessage('Erro: Credenciais inválidas.');
        } else {
             setMessage(`Erro: ${error.response.data.error || 'Falha na comunicação com a API.'}`);
        }
      } else {
        setMessage('Erro de rede ou interno.');
      }
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <form onSubmit={handleLogin} className="p-6 bg-white rounded shadow-md w-96">
        <h2 className="text-2xl font-bold mb-4">Acesso NexoPark</h2>
        
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700">Email:</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="mt-1 p-2 w-full border rounded-md"
            required
          />
        </div>
        
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700">Senha:</label>
          <input
            type="password"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            className="mt-1 p-2 w-full border rounded-md"
            required
          />
        </div>
        
        <button
          type="submit"
          className="w-full bg-blue-500 text-white p-2 rounded-md hover:bg-blue-600"
        >
          Login
        </button>
        
        {message && <p className="mt-4 text-center text-sm font-medium">{message}</p>}
      </form>
    </div>
  );
}