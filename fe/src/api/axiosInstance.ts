import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:3000', // Replace with your microservices API gateway or base URL
  headers: {
    'Content-Type': 'application/json',
  },
});

export default apiClient;
