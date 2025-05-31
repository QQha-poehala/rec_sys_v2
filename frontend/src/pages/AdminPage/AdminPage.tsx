import React, { useEffect, useState } from "react";
import "./AdminPage.css";
import NSTUlogo from '../../assets/pics/logo_white.png';
import { useLocation, useNavigate } from "react-router-dom";
import { ExportToCsvService } from "../../shared/services/ExportTocsvService";
import { GetDataService } from "../../shared/services/GetDataService";


interface TableData {
  id: string;
  createdAt: string;
  inputData: any;
  result1: string;
  result2: string;
  result3: string;
}

interface PagedResult {
  totalCount: number;
  page: number;
  items: TableData[];
}

const AdminPage: React.FC = () => {
  const [pagedData, setPagedData] = useState<PagedResult | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    if (!location.state) {
      navigate("/admin");
    } else {
      fetchData(currentPage);
    }
  }, []);


  const fetchData = async (page: number) => {
    setIsLoading(true);
    setError(null);
    try {
        const result = await GetDataService.getData(page);
        setPagedData(result);
    } catch (err) {
        setError("Ошибка при загрузке данных");
        console.error(err);
    } finally {
        setIsLoading(false);
    }
  };
  const handlePageChange = (newPage: number) => {
    if (newPage < 1 || (pagedData && newPage > Math.ceil(pagedData.totalCount / 10))) return;
    setCurrentPage(newPage);
    fetchData(newPage);
  };


  const exportToCSV = async () => {
    try {
      const blob = await ExportToCsvService.export();
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "student_responses.csv";
      a.click();
      URL.revokeObjectURL(url);
    } catch (error) {
      alert("Ошибка при экспорте");
      console.error(error);
    }
  };

  const shouldShowPagination = () => {
    return pagedData && pagedData.totalCount > 10;
  };

  const renderTable = () => {
    if (!pagedData || pagedData.items.length === 0) {
        return <div className="no-data">Данных нет</div>;
    }

    return (
        <>
            <table className="admin-table">
                <thead>
                    <tr>
                        <th>Дата</th>
                        <th className="input-data-column">Данные анкеты</th>
                        <th>Рекомендация 1</th>
                        <th>Рекомендация 2</th>
                        <th>Рекомендация 3</th>
                    </tr>
                </thead>
                <tbody>
                    {pagedData.items.map((row) => (
                        <tr key={row.id}>
                            <td>{new Date(row.createdAt).toLocaleString()}</td>
                            <td>
                                <div className="json-container">
                                <pre>{JSON.stringify(row.inputData, null, 2)}</pre>
                                </div>
                            </td>
                            <td>{row.result1}</td>
                            <td>{row.result2}</td>
                            <td>{row.result3}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
            
            {shouldShowPagination() && (
                <div className="pagination-controls">
                    <button 
                        className="pagination-button"
                        onClick={() => handlePageChange(currentPage - 1)}
                        disabled={currentPage === 1 || isLoading}
                    >
                        Назад
                    </button>
                    <span className="page-number">Страница {currentPage}</span>
                    <button 
                        className="pagination-button"
                        onClick={() => handlePageChange(currentPage + 1)}
                        disabled={!pagedData || currentPage * 10 >= pagedData.totalCount || isLoading}
                    >
                        Вперед
                    </button>
                </div>
            )}
        </>
    );
};

return (
    <div className="admin-container">
        <img src={NSTUlogo} alt="logo" className="logo" />
        <div className="admin-panel">
            Админ-панель
        </div>
        
        <div className="admin-actions">
            <button onClick={exportToCSV} className="admin-button">
                Экспорт в CSV
            </button>
            {pagedData && (
                <div className="total-count">
                    Всего записей: {pagedData.totalCount}
                </div>
            )}
        </div>

        {isLoading ? (
            <div className="loading">Загрузка данных...</div>
        ) : error ? (
            <div className="error">{error}</div>
        ) : (
            renderTable()
        )}
    </div>
);
};

export default AdminPage;