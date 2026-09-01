import requests
import streamlit as st
from typing import Dict, List, Optional, Tuple, Any

COINGECKO_BASE = "https://api.coingecko.com/api/v3"
DEFAULT_TIMEOUT = 10

HEADERS = {
    "User-Agent": "CryptoDashboardCV/1.0 (Portfolio Project)"
}

@st.cache_data(ttl=300, show_spinner=False)
def fetch_top_coins(vs_currency: str = "usd", top_n: int = 10) -> Tuple[Optional[List[Dict[str, Any]]], bool]:
    """
    Fetch top coins by market cap from CoinGecko.
    Returns (data_list, is_fallback_boolean).
    """
    url = f"{COINGECKO_BASE}/coins/markets"
    params = {
        "vs_currency": vs_currency.lower(),
        "order": "market_cap_desc",
        "per_page": top_n,
        "page": 1,
        "sparkline": "false",
        "price_change_percentage": "24h,7d"
    }
    
    try:
        response = requests.get(url, params=params, headers=HEADERS, timeout=DEFAULT_TIMEOUT)
        if response.status_code == 200:
            return response.json(), False
        elif response.status_code == 429:
            st.warning("⚠️ Límite de peticiones de API pública alcanzado (Rate Limit). Usando dataset en caché/simulado.")
            return None, True
        else:
            return None, True
    except Exception:
        return None, True


@st.cache_data(ttl=300, show_spinner=False)
def fetch_coin_history(coin_id: str, vs_currency: str = "usd", days: int = 30) -> Tuple[Optional[Dict[str, Any]], bool]:
    """
    Fetch historical prices and volumes for a given coin.
    Returns (raw_history_dict, is_fallback_boolean).
    """
    url = f"{COINGECKO_BASE}/coins/{coin_id}/market_chart"
    params = {
        "vs_currency": vs_currency.lower(),
        "days": days,
        "interval": "daily" if days > 1 else "hourly"
    }
    
    try:
        response = requests.get(url, params=params, headers=HEADERS, timeout=DEFAULT_TIMEOUT)
        if response.status_code == 200:
            return response.json(), False
        else:
            return None, True
    except Exception:
        return None, True
