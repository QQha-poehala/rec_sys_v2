export const GetDataService = {
  async getData(page: number = 1): Promise<any> {
    try {
      const response = await fetch(`${process.env.REACT_APP_API_URI}/get_data_for_table?page=${page}`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error("Ошибка при получении данных:", error);
      throw new Error("Ошибка при получении данных");
    }
  }
};