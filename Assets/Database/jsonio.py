import requests

URL = 'https://api.jsonbin.io/v3/b'

HEADERS = {
  'Content-Type': 'application/json',
  'X-Master-Key': '$2a$10$Z6B/b820H97jIfVer0qXVem.TJ5c69MZdAppCDKQ/1C6KwBtYYX/2'
}

def print_status(func):
    def wrapper(*args, **kwargs):
        print(f"Executing {func.__name__}...")
        result = func(*args, **kwargs)
        if result is not None:
            print("Success.")
        else:
            print("Failed.")
        return result
    return wrapper

@print_status
def create_data():
    session = requests.Session()
    try:
        response = session.post(URL, json={"empty": "empty"}, headers=HEADERS)
        response.raise_for_status()  # Raise exception for non-200 status codes
        bin_id = response.json()['metadata']['id']
        return bin_id
    except requests.exceptions.RequestException as e:
        print(f"Error occurred: {e}")
        return None
    finally:
        session.close()

@print_status
def update_data(bin_id, json_data):
    session = requests.Session()
    try:
        cache_url = f"{URL}/{bin_id}"
        response = session.put(cache_url, json=json_data, headers=HEADERS)
        response.raise_for_status()  # Raise exception for non-200 status codes
        return True
    except requests.exceptions.RequestException as e:
        print(f"Error occurred: {e}")
        return False
    finally:
        session.close()