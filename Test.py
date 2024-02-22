from imblearn.over_sampling import SMOTE
from imblearn.under_sampling import RandomUnderSampler
from matplotlib import colors
import pandas as pd
import numpy as np
import math
import matplotlib.pyplot as plt

# Caricamento dataset
df = pd.read_excel(r"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\Partitioned Dataset Anacredit.xlsx")
print(df.info())
# Mappatura colonna 'Valori anomali' in 0 per non anomali e 1 per anomali
df['Valori anomali'] = np.where(df['Valori anomali'] == 'anomali', 1, 0)

# Colonna target sia di tipo numerico
df['Valori anomali'] = df['Valori anomali'].astype('int')

# Divisione dataset in feature (X) e target (y)
X = df.drop('Valori anomali', axis=1)
y = df['Valori anomali']

# Mappa delle etichette
label_mapping = {0: 'Regolare', 1: 'Anomalo'}
# Applicazione mappatura delle etichette
AnomalyLabels = y.map(label_mapping)

# Rimuovi temporaneamente le colonne univoche
unique_cols = ['CNTRCT_ID(ID)', 'INSTRMNT_ID(ID)']
X_without_unique_cols = X.drop(unique_cols, axis=1)

# Ottieni il numero di righe del dataframe originale
num_samples_original = df.shape[0]

smotestrategy = [0.041, 0.082, 0.164, 0.240, 0.3247,0.401, 0.4875, 0.561,0.642, 0.722, 0.803,0.882,1]

for i in range(len(smotestrategy)):
    # Inizializza l'oggetto SMOTE per l'oversampling (parto da 50 campioni e raddoppio progressivamente)
    smote = SMOTE(sampling_strategy= smotestrategy[i],random_state=42)

    
    if smotestrategy[i] == 0.041:
        num_samples = int(round(smotestrategy[i] * len(X), -1))
    elif smotestrategy[i] == 1:
        num_samples = num_samples_original  
    else:
        num_samples = int(round(smotestrategy[i] * len(X), -2))
    
    # Esegui l'oversampling
    X_resampled, y_resampled = smote.fit_resample(X_without_unique_cols, y)

    # Unisci nuovamente le colonne univoche al DataFrame risultante
    X_resampled = pd.concat([X_resampled, X[unique_cols]], axis=1)

    # Crea un nuovo DataFrame con il dataset bilanciato
    df_resampled = pd.concat([X_resampled, pd.Series(y_resampled, name='Valori anomali')], axis=1)

    # Aggiungi un nuovo indice progressivo solo per le nuove istanze generate durante l'oversampling
    df_resampled['CNTRCT_ID(ID)'] = range(1, 1 + len(df_resampled))
    df_resampled['INSTRMNT_ID(ID)'] = range(1, 1 + len(df_resampled))

    # Ordine desiderato delle colonne
    desired_columns_order = ['CNTRCT_ID(ID)', 'INSTRMNT_ID(ID)', 'Codice Controparte', 'TypeofInstrument', 'ImpairmentStatus', 'ONA', 'Valori anomali']

    # Reimposta l'ordine delle colonne nel DataFrame
    df_resampled = df_resampled[desired_columns_order]

    #Resetta l'indice, dà un ordinamento randomico
    df_resampled = df_resampled.sample(frac=1, random_state=42).reset_index(drop=True)

    # Stampa le prime righe del DataFrame risultante
    print(df_resampled.head())

    Anomalies=y_resampled.value_counts()

    plt.figure(figsize=(8, 8))
    Anomalies.plot.pie(autopct='%.2f', labels=None, startangle=60,colors=['Blue','Red'])
    # Aggiungta etichette al grafico
    plt.title("OverSampling")
    plt.legend(labels=AnomalyLabels.value_counts().index, loc="upper right")
    plt.show()


    # Salva il dataframe in un nuovo file Excel

    filename = f"C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\PreparedDatasets\\{num_samples}SamplesDataset.xlsx"

    try:
        df_resampled.to_excel(filename, index=False)
        print(f"File salvato con successo: {filename}")
    except Exception as e:
        print(f"Errore durante il salvataggio del file Excel: {str(e)}")

