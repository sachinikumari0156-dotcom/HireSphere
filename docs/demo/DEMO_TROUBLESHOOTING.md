# Demo troubleshooting

| Symptom | Likely cause | Action |
|---------|--------------|--------|
| Frontend cannot call API | Wrong `VITE_API_BASE_URL` / CORS | Align origin and API URL |
| Login 401 | Wrong password / disabled user | Use local seed accounts |
| EF errors | LocalDB not started | `sqllocaldb start MSSQLLocalDB` |
| Upload rejected | Validation rules | Use allowed extension/size |
| External calendar empty | Not Configured | Show internal/ICS path |
| Port in use | Stale `dotnet`/`node` | Stop process on 5167/5173 |
| Blank page | JS error | Check browser console; ensure build matches |

Never paste real JWT keys or connection strings into shared chat logs.
