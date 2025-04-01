export const ExportToCsvService = {
  async export(): Promise<Blob> {
    const response = await fetch(`${process.env.REACT_APP_API_URI}/getfile`, {
      method: "GET"
    });

    if (!response.ok) {
      throw new Error("Не удалось получить файл с сервера");
    }
    return await response.blob();
  }
};
