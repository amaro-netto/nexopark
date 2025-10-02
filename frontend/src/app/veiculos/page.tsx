'use client';
import { useState, useEffect } from 'react';
import axios from 'axios';

// Configuração Base: Endereço da sua API
const API_BASE_URL = 'http://localhost:5196'; 

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
  
  // NOTE: Em produção, o token seria lido de um HttpOnly Cookie
  // Aqui, vamos usar um token mockado APENAS para testar a requisição
  const MOCK_TOKEN = "DEBUG_TOKEN_ADMIN"; 

  useEffect(() => {
    const fetchVeiculos = async () => {
      // 1. Verificar se estamos logados (simulação)
      if (!MOCK_TOKEN) {
        setError("Não autorizado. Por favor, faça login.");
        setLoading(false);
        return;
      }

      try {
        const response = await axios.get(`${API_BASE_URL}/veiculos`, {
          headers: {
            // 2. Envia o token para a API para autorização
            Authorization: `Bearer ${MOCK_TOKEN}`, 
          }
        });
        setVeiculos(response.data);
      } catch (err) {
        if (axios.isAxiosError(err) && err.response) {
          if (err.response.status === 401 || err.response.status === 403) {
            setError("Você não tem permissão para visualizar esta página.");
          } else {
            setError(`Erro ao carregar dados: ${err.response.statusText}`);
          }
        } else {
          setError('Erro de rede.');
        }
      } finally {
        setLoading(false);
      }
    };
    
    fetchVeiculos();
  }, []);

  if (loading) return <div className="text-center mt-8">Carregando veículos...</div>;
  if (error) return <div className="text-center mt-8 text-red-600">Erro: {error}</div>;

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">Lista de Veículos Registrados</h1>
      <p className="mb-4 text-sm text-gray-600">
        (DEBUG: Usando MOCK_TOKEN para testar a rota protegida.)
      </p>
      
      {veiculos.length === 0 ? (
        <p className="text-center text-gray-500">Nenhum veículo encontrado. Crie um novo via Swagger.</p>
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