(function () {
    const root = document.getElementById('etapas');
    if (!root) return;

    const ingredientes = JSON.parse(root.dataset.ingredientes);
    const unidades = JSON.parse(root.dataset.unidades);
    const inicial = JSON.parse(root.dataset.inicial || '[]');
    let contador = 0;

    const esc = s => String(s ?? '').replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;');
    const tipoDe = id => (ingredientes.find(i => i.id == id) || ingredientes[0] || {}).tipo;

    const ingOptions = sel => ingredientes
        .map(i => `<option value="${i.id}" ${i.id == sel ? 'selected' : ''}>${esc(i.nome)}</option>`).join('');

    const uniOptions = (tipo, sel) => {
        const lista = unidades.filter(u => u.tipo === tipo);
        const valido = lista.some(u => u.v == sel);
        return lista.map((u, idx) =>
            `<option value="${u.v}" ${(valido ? u.v == sel : idx === 0) ? 'selected' : ''}>${u.t}</option>`).join('');
    };

    function renumerar() {
        [...root.children].forEach((card, n) => {
            card.querySelector('.etapa-titulo').textContent = 'Etapa ' + (n + 1);
        });
    }

    function addIngrediente(card, dados) {
        const e = card.dataset.idx;
        const j = card._n++;
        const row = document.createElement('div');
        row.className = 'row g-2 mb-2 ing-row';
        const ingId = dados?.ingredienteId;
        const tipo = tipoDe(ingId);
        row.innerHTML = `
            <div class="col-md-5">
                <select class="form-select ing-sel" name="Etapas[${e}].Ingredientes[${j}].IngredienteId" aria-label="Ingrediente">${ingOptions(ingId)}</select>
            </div>
            <div class="col-6 col-md-3">
                <input type="number" step="any" min="0.001" class="form-control" required aria-label="Quantidade"
                       name="Etapas[${e}].Ingredientes[${j}].Quantidade" value="${dados?.quantidade > 0 ? dados.quantidade : ''}" />
            </div>
            <div class="col-4 col-md-3">
                <select class="form-select uni-sel" name="Etapas[${e}].Ingredientes[${j}].Unidade" aria-label="Unidade">${uniOptions(tipo, dados?.unidade)}</select>
            </div>
            <div class="col-2 col-md-1">
                <button type="button" class="btn btn-outline-danger w-100 remover-ing" aria-label="Remover ingrediente">×</button>
            </div>`;
        card.querySelector('.ings').appendChild(row);
    }

    function addEtapa(dados) {
        const card = document.createElement('div');
        card.className = 'panel mb-3';
        card.dataset.idx = contador++;
        card._n = 0;
        card.innerHTML = `
            <div class="d-flex justify-content-between align-items-center mb-2">
                <strong class="etapa-titulo"></strong>
                <button type="button" class="btn btn-sm btn-outline-danger remover-etapa">Remover etapa</button>
            </div>
            <textarea class="form-control mb-3" rows="2" required placeholder="O que fazer nesta etapa"
                      name="Etapas[${card.dataset.idx}].Descricao">${esc(dados?.descricao)}</textarea>
            <div class="ings"></div>
            <button type="button" class="btn btn-sm btn-outline-secondary add-ing">Adicionar ingrediente</button>`;
        root.appendChild(card);
        (dados?.ingredientes || []).forEach(i => addIngrediente(card, i));
        renumerar();
    }

    root.addEventListener('click', ev => {
        const alvo = ev.target;
        if (alvo.closest('.add-ing')) addIngrediente(alvo.closest('.panel'));
        else if (alvo.closest('.remover-ing')) alvo.closest('.ing-row').remove();
        else if (alvo.closest('.remover-etapa')) { alvo.closest('.panel').remove(); renumerar(); }
    });

    // Ao trocar o ingrediente, só oferece unidades compatíveis (g/kg ou ml/L)
    root.addEventListener('change', ev => {
        if (!ev.target.classList.contains('ing-sel')) return;
        const row = ev.target.closest('.ing-row');
        const uni = row.querySelector('.uni-sel');
        uni.innerHTML = uniOptions(tipoDe(ev.target.value), uni.value);
    });

    document.getElementById('add-etapa').addEventListener('click', () => addEtapa());

    // Remover etapas/ingredientes deixa "buracos" nos índices; reindexa antes de enviar.
    document.getElementById('form-receita').addEventListener('submit', () => {
        [...root.children].forEach((card, e) => {
            card.querySelectorAll('[name]').forEach(el => {
                el.name = el.name.replace(/^Etapas\[\d+\]/, `Etapas[${e}]`);
            });
            card.querySelectorAll('.ing-row').forEach((row, j) => {
                row.querySelectorAll('[name]').forEach(el => {
                    el.name = el.name.replace(/\.Ingredientes\[\d+\]/, `.Ingredientes[${j}]`);
                });
            });
        });
    });

    if (inicial.length) inicial.forEach(addEtapa); else addEtapa();
})();
