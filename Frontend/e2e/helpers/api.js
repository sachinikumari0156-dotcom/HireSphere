import { e2eEnv } from "./env.js";

export async function apiRequest(request, method, route, { token, data, multipart } = {}) {
  const { apiBase } = e2eEnv();
  const headers = {};
  if (token) headers.Authorization = `Bearer ${token}`;

  const options = { method, headers };
  if (multipart) {
    options.multipart = multipart;
  } else if (data !== undefined) {
    headers["Content-Type"] = "application/json";
    options.data = data;
  }

  const response = await request.fetch(`${apiBase}${route}`, options);
  let body = null;
  const text = await response.text();
  if (text) {
    try {
      body = JSON.parse(text);
    } catch {
      body = text;
    }
  }
  return { ok: response.ok(), status: response.status(), body };
}

export async function ensureCatalog(request) {
  return apiRequest(request, "POST", "/e2e/ensure-catalog");
}

export async function prepareCandidateJourney(request, candidateEmail) {
  return apiRequest(request, "POST", "/e2e/prepare-candidate-journey", {
    data: { candidateEmail }
  });
}

export async function registerCandidate(request, { email, password, firstName = "E2E", lastName = "Candidate" }) {
  return apiRequest(request, "POST", "/auth/register/candidate", {
    data: {
      firstName,
      lastName,
      email,
      password,
      confirmPassword: password,
      acceptTerms: true
    }
  });
}

export async function login(request, email, password) {
  return apiRequest(request, "POST", "/auth/login", {
    data: { email, password }
  });
}
