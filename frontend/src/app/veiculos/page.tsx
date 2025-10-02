'use client';
import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { useRouter } from 'next/navigation';
import NovoVeiculo from './NovoVeiculo'; // Importa o novo componente

const API_BASE_URL = '/api'; 

interface Veiculo {
  id: string;
  placa: string;
  modelo: string;
  cor: string;
  dataRegistro: string;
  administrador?: {
    nome: string;
  };
}

export default function VeiculosPage() {
  const [veiculos, setVeiculos] = useState<Veiculo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [token, setToken] = useState<string | null>(null); // Estado para o token
  const router = useRouter(); 

  // Função para buscar veículos - será chamada no load e após o registro
  const fetchVeiculos = useCallback(async (authToken: string) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/veiculos`, {
        headers: {
          Authorization: `Bearer ${authToken}`, 
        }
      });
      setVeiculos(response.data);
    } catch (err) {
      if (axios.isAxiosError(err) && (err.response?.status === 401 || err.response?.status === 403)) {
        // Token expirou/foi rejeitado: limpa e redireciona para login
        localStorage.removeItem('userToken');
        router.push('/login');
        return;
      }
      setError('Erro ao carregar dados. Verifique a API.');
    } finally {
      setLoading(false);
    }
  }, [router]);

  // Efeito principal: verifica token e inicia a busca
  useEffect(() => {
    const storedToken = localStorage.getItem('userToken');
    setToken(storedToken); // Atualiza o estado do token

    if (!storedToken) {
      router.push('/login');
      return;
    }
    
    // Inicia a busca (com o token)
    fetchVeiculos(storedToken);
  }, [router, fetchVeiculos]);

  const handleLogout = () => {
    localStorage.removeItem('userToken');
    router.push('/login');
  };
  
  // Função para recarregar a lista (passada para o NovoVeiculo)
  const handleVeiculoCreated = () => {
      // Recarrega a lista após a criação bem-sucedida
      if(token) fetchVeiculos(token); 
  }

  if (loading || !token) return <div className="text-center mt-8">Carregando...</div>;
  if (error) return <div className="text-center mt-8 text-red-600">Erro: {error}</div>;

  return (
    <div className="container mx-auto p-8">
      <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold">Gestão de Veículos</h1>
          <button 
            onClick={handleLogout} 
            className="bg-red-500 text-white p-2 rounded hover:bg-red-600 transition"
          >
            Logout
          </button>
      </div>

      <NovoVeiculo onSuccess={handleVeiculoCreated} token={token} /> 

      <h2 className="text-2xl font-bold mb-4">Lista de Registros</h2>
      <p className="mb-4 text-sm text-red-600 font-bold">
        ⚠️ ATENÇÃO: Token armazenado no localStorage. **INSEGURO** para produção.
      </p>
      
      {veiculos.length === 0 ? (
        <p className="text-center text-gray-500 p-10 border rounded-lg">
            Nenhum veículo encontrado. Registre um novo acima.
        </p>
      ) : (
        <table className="min-w-full bg-white shadow-md rounded-lg overflow-hidden">
          <thead className="bg-gray-800 text-white">
            <tr>
              <th className="py-2 px-4">Placa</th>
              <th className="py-2 px-4">Modelo</th>
              <th className="py-2 px-4">Cor</th>
              <th className="py-2 px-4">Registrado Por</th>
              <th className="py-2 px-4">Data Registro</th>
            </tr>
          </thead>
          <tbody>
            {veiculos.map((v) => (
              <tr key={v.id} className="border-b hover:bg-gray-50">
                <td className="py-2 px-4 font-bold">{v.placa}</td>
                <td className="py-2 px-4">{v.modelo}</td>
                <td className="py-2 px-4">{v.cor}</td>
                <td className="py-2 px-4">{v.administrador?.nome || 'N/A'}</td>
                <td className="py-2 px-4">{new Date(v.dataRegistro).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}