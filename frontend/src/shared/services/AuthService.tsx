import { AuthModel } from "../models/AuthModel";

export const AuthService = {
  async Auth(data: AuthModel): Promise<{success: boolean; data?: any; error?: string }> {
    try {
      const response = await fetch(`${process.env.REACT_APP_API_URI}/auth`, {
        method: "POST",
        body: JSON.stringify(data),
        mode: "cors",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (response.ok) {
        const responseData = await response.json();
        return { success: true, data: responseData };
      } else {
        const errorData = await response.json().catch(() => null);
        return { 
          success: false, 
          error: errorData?.message || "Ошибка авторизации" 
        };
      }
    } catch (error) {
      console.error("Ошибка запроса:", error);
      return { 
        success: false, 
        error: "Ошибка соединения с сервером" 
    };
    }
  },
};