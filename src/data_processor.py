import pandas as pd
import numpy as np
from datetime import datetime, timedelta
from typing import Dict, List, Any, Tuple

# Fallback crypto metadata
FALLBACK_COINS = [
    {"id": "bitcoin", "symbol": "btc", "name": "Bitcoin", "current_price": 64500.0, "market_cap": 1270000000000, "price_change_percentage_24h": 2.45, "total_volume": 28500000000, "high_24h": 65200.0, "low_24h": 63100.0},
    {"id": "ethereum", "symbol": "eth", "name": "Ethereum", "current_price": 3450.0, "market_cap": 415000000000, "price_change_percentage_24h": -1.15, "total_volume": 14200000000, "high_24h": 3520.0, "low_24h": 3390.0},
    {"id": "solana", "symbol": "sol", "name": "Solana", "current_price": 178.5, "market_cap": 83000000000, "price_change_percentage_24h": 5.80, "total_volume": 4100000000, "high_24h": 182.0, "low_24h": 168.0},
    {"id": "cardano", "symbol": "ada", "name": "Cardano", "current_price": 0.42, "market_cap": 15000000000, "price_change_percentage_24h": 0.75, "total_volume": 520000000, "high_24h": 0.435, "low_24h": 0.41},
    {"id": "ripple", "symbol": "xrp", "name": "XRP", "current_price": 0.58, "market_cap": 32000000000, "price_change_percentage_24h": 1.20, "total_volume": 1100000000, "high_24h": 0.60, "low_24h": 0.56}
]

def generate_fallback_history(coin_id: str, days: int = 30) -> pd.DataFrame:
    """
    Generates a realistic synthetic historical price & volume DataFrame when API is unreachable.
    """
    base_price = 60000.0 if coin_id == "bitcoin" else (3200.0 if coin_id == "ethereum" else 160.0)
    dates = [datetime.now() - timedelta(days=i) for i in range(days, -1, -1)]
    
    np.random.seed(42 + len(coin_id))
    returns = np.random.normal(loc=0.001, scale=0.025, size=len(dates))
    price_path = base_price * np.cumprod(1 + returns)
    volumes = np.random.uniform(low=1e9, high=3e10, size=len(dates))
    
    df = pd.DataFrame({
        "timestamp": dates,
        "price": price_path,
        "total_volume": volumes
    })
    return add_technical_indicators(df)


def process_top_coins(raw_coins: List[Dict[str, Any]]) -> pd.DataFrame:
    """
    Transforms raw coin market JSON into a clean Pandas DataFrame.
    """
    if not raw_coins:
        return pd.DataFrame(FALLBACK_COINS)
    
    df = pd.DataFrame(raw_coins)
    cols = ["id", "name", "symbol", "current_price", "market_cap", "price_change_percentage_24h", "total_volume", "high_24h", "low_24h"]
    available_cols = [c for c in cols if c in df.columns]
    df = df[available_cols].copy()
    
    # Fill missing values if any
    df["price_change_percentage_24h"] = df["price_change_percentage_24h"].fillna(0.0)
    df["symbol"] = df["symbol"].str.upper()
    return df


def process_history(raw_history: Dict[str, Any]) -> pd.DataFrame:
    """
    Transforms CoinGecko history response dictionary into a cleaned DataFrame with timestamps.
    """
    prices = raw_history.get("prices", [])
    volumes = raw_history.get("total_volumes", [])
    
    if not prices:
        return generate_fallback_history("bitcoin", 30)
    
    df_prices = pd.DataFrame(prices, columns=["timestamp_ms", "price"])
    df_volumes = pd.DataFrame(volumes, columns=["timestamp_ms", "total_volume"])
    
    df = pd.merge(df_prices, df_volumes, on="timestamp_ms", how="inner")
    df["timestamp"] = pd.to_datetime(df["timestamp_ms"], unit="ms")
    df.drop(columns=["timestamp_ms"], inplace=True)
    df.sort_values(by="timestamp", inplace=True)
    df.reset_index(drop=True, inplace=True)
    
    return add_technical_indicators(df)


def add_technical_indicators(df: pd.DataFrame) -> pd.DataFrame:
    """
    Calculates technical indicators: Simple Moving Averages (SMA 7 & SMA 30),
    Daily Returns (%), and Volatility (std dev of returns).
    """
    df = df.copy()
    if "price" not in df.columns:
        return df
    
    # Rolling Moving Averages
    df["SMA_7"] = df["price"].rolling(window=min(7, len(df)), min_periods=1).mean()
    df["SMA_30"] = df["price"].rolling(window=min(30, len(df)), min_periods=1).mean()
    
    # Percentage daily return
    df["daily_return"] = df["price"].pct_change() * 100
    df["daily_return"] = df["daily_return"].fillna(0.0)
    
    # Volatility (Rolling 7-period standard deviation of returns)
    df["volatility_7d"] = df["daily_return"].rolling(window=min(7, len(df)), min_periods=1).std().fillna(0.0)
    
    return df


def format_currency(val: float, symbol: str = "$") -> str:
    """Helper to format currency strings cleanly."""
    if val >= 1e9:
        return f"{symbol}{val / 1e9:,.2f}B"
    elif val >= 1e6:
        return f"{symbol}{val / 1e6:,.2f}M"
    elif val >= 1.0:
        return f"{symbol}{val:,.2f}"
    else:
        return f"{symbol}{val:,.4f}"
