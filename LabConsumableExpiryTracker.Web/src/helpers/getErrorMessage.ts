type ApiErrorData = {
  errors?: Record<string, string[] | string>;
  detail?: string;
  title?: string;
  message?: string;
};


export const getErrorMessage = (
  error: unknown,
  fallback = "Something went wrong. Please try again."
): string => {
  const data = (error as { response?: { data?: unknown } } | null)?.response
    ?.data;

  if (typeof data === "string" && data.trim()) {
    return data;
  }

  if (data && typeof data === "object") {
    const parsed = data as ApiErrorData;

    if (parsed.errors && typeof parsed.errors === "object") {
      const messages = Object.values(parsed.errors).flat().filter(Boolean);

      if (messages.length > 0) {
        return messages.join(" ");
      }
    }

    const message = parsed.detail ?? parsed.title ?? parsed.message;

    if (message) {
      return message;
    }
  }

  const generic = (error as { message?: string } | null)?.message;

  return generic || fallback;
};