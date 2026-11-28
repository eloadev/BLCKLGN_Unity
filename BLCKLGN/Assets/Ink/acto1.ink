// =====================================================
// ACTO 1 — BLCKLGN
// 1.1 La entrevista  /  1.2 La reunión en casa  /  1.3 Tras bambalinas
// Sin QTE. Decisiones ilusorias en 1.2 y 1.3.
//
// RECORDATORIOS:
// - Un tag en una línea sola se pega a la SIGUIENTE línea de texto.
// - Las líneas que empiezan con @ son "líneas de acción": Unity NO
//   muestra caja de texto, solo ejecuta sus tags y avanza solo
//   (si tienen #wait, espera ese tiempo antes de avanzar).
// - Toda línea de diálogo lleva su #speaker (aunque repita).
// =====================================================


// -----------------------------------------------------
// 1.1 — INT. SET DE GRABACIÓN — DÍA
// -----------------------------------------------------
=== a1_1_entrevista ===
#bg: lente_camara
#amb: foro_zumbido
#title: 1.1: La entrevista
#cam: zoom_out_lento
—Al aire en 5… 4… 3… 2… #speaker: camarografo_vo
-> presentacion

= presentacion
#transition: corte_directo
#bg: set_tv
#sfx: aplausos_publico
#sprite: presentador_sonrisa
—¡Señoras y señores, lo prometido es deuda! Por primera vez en la ciudad, arrancando su gira mundial con el álbum VoidStars… ¡Con ustedes, BLACK LAGOON! #speaker: presentador

// Entran Alex, Bennedict y Noah
@entrada_banda #sfx: ovacion #cam: paneo_entrada #sprite: alex_arrogante #sprite: bennedict_arrogante #sprite: noah_arrogante #wait: 2

—¡Bienvenidos! Qué emoción tenerlos aquí. ¿Cómo los trata la ciudad? #speaker: presentador
#sprite: bennedict_sillon
—Bien, bien... Oye, una pregunta, ¿quién de tu staff me puede conseguir un trago? Un Jack Daniel's, en las rocas. #speaker: bennedict
#sprite: presentador_incomodo
—Ah... bueno, no estoy seguro, pero podemos revisarlo... #speaker: presentador
#sprite: alex_cigarro
—¿No tienes un encendedor? #speaker: alex
#sprite: presentador_nervioso
—¡Oh, no, no! Es un espacio libre de humo, apto para toda la familia. #speaker: presentador
—Qué puta mierda. ¿Benny puede pedir alcohol pero yo no puedo fumar? #speaker: alex
#sfx: murmullo_incomodo
#sprite: noah_burla
—¡Te lo dije! Te dije que no te dejarían. Perdiste la apuesta, imbécil, me debes dinero. #speaker: noah
-> inspiracion

= inspiracion
#sfx: carraspeo
#sprite: presentador_sonrisa
—Bueno... cambiando de tema. Tengo que preguntar lo evidente. Viendo su ropa y escuchando los tonos espaciales y, digamos, "psicodélicos" de su nuevo disco... ¿De dónde es que nacen todos estos conceptos? #speaker: presentador
#sprite: bennedict_serio
—De que tengo mucho calor. #speaker: bennedict
#sfx: risa_nerviosa_publico
—¿Calor? ¿A 12 grados con lluvia? #speaker: presentador
—Seh... Nuestra primera vez aquí y no es de oquis. Pero si quieres la verdad del disco: un día me levanté completamente drogado. Todo daba vueltas y veía figuras extrañas en el aire. #speaker: bennedict
—Llegué pedísimo a casa de un amigo y vi cómo este vergas, osea, Noah, intentaba ligarse a una mujer casada. El marido le metió un golpe tan fuerte que cayó de hocico al pavimento. #speaker: bennedict
#sprite: noah_enojado
—Oye, con todo respeto, esa es la historia más pendeja que he escuchado. ¡Ni siquiera pasó así! Ella se me acercó y... #speaker: noah
—Cierra el hocico, todos vimos lo que pasó. #speaker: alex
—¡A ti nadie te está hablando, metiche de mierda! #speaker: noah
#sprite: bennedict_reto
—El punto es que vi a Noah caer en cámara lenta. Mientras su sangre salpicaba el suelo, el riff de Fallen Star empezó a sonar en mi cabeza. #speaker: bennedict
—Cuando eres así de bueno, la genialidad simplemente ocurre. No espero que lo entiendan. #speaker: bennedict
#cam: primer_plano_presentador
#sprite: presentador_perplejo
—Claro... una inspiración muy peculiar. Por cierto, notamos que falta alguien. ¿No son cuatro en la banda? #speaker: presentador
-> roses_irrumpe

= roses_irrumpe
#sfx: estruendo_backstage
#cam: shake_leve
#sprite: roses_botella
—¡Qué tal, pendejitos! ¿Qué dijeron? ¿Que no iba a llegar? #speaker: roses
#sprite: presentador_de_pie
—¡Oye, no puedes entrar así, estamos al aire! #speaker: presentador
#cg: roses_humo_presentador
—Ay, ¿de verdad? Qué mal... #speaker: roses
#cg: ocultar
#sfx: trago_botella
#sprite: noah_harto
—Loco hijo de perra, ya siéntate de una vez. #speaker: noah
-> caos

= caos
// Roses lanza la botella (cámara lenta 0.5x)
@botella_lanzada #cam: camara_lenta #sprite: bennedict_sonrisa #wait: 1.5
@reflector_explota #sfx: estallido_electrico #vfx: chispas #vfx: apagon_fondo #cam: shake_leve #wait: 1
@caos_staff #amb: gritos_staff #sfx: risa_roses #sfx: encendedor #wait: 1.5
// Cierre: solo los cuatro en cuadro
@cierre_escena #cam: encuadre_banda #sprite: roses_riendo #sprite: alex_fumando #sprite: bennedict_espejo #wait: 2.5
-> a1_2_reunion


// -----------------------------------------------------
// 1.2 — INT. SALA DE LA MANSIÓN — DÍA
// -----------------------------------------------------
=== a1_2_reunion ===
#transition: disolver
#hide: todos
#bg: mansion_mesa_periodico
#amb: tv_bajo
#title: 1.2: La reunión en casa
#sfx: periodico_arrugado
—Mierda… #speaker: oliver
—Y en primera plana... ¡¿Cuál es su puto problema?! #speaker: oliver
-> sala

= sala
#bg: mansion_sala
#sprite: noah_dormido
#sprite: bennedict_tv
—¡¿Sigo sin entender qué puta mierda les pasa por la cabeza?! #speaker: oliver
@periodico_a_noah #sfx: periodico_golpe #wait: 1
-> alex_ventana

= alex_ventana
#cam: desplazamiento_ventana
#hide: todos
#sprite: alex_vacio
—En serio... ¿qué mierdas les pasa por la cabeza? #speaker: oliver
—Alguna vez... ¿has pensado sobre qué sería de ti si no fueras tú? #speaker: alex

// DECISIÓN ILUSORIA: cualquier opción lleva a la misma respuesta de Oliver
* [Pedirle que deje de filosofar.]
* [Preguntarle qué le pasa.]
* [Ignorar la pregunta.]
- —Güey, ¿de qué mierda estás hablando? Te pregunté una cosa, me lleva la chingada... #speaker: oliver
-> roses_bano

= roses_bano
#sfx: puerta_abre
#sprite: roses_cierre_abajo
—¡El señorón mánager! ¡El hombre, el mito, la leyenda! #speaker: roses
#sfx: palmadas_pecho
—Dime que por favor te lavaste las putas manos antes de tocarme. #speaker: oliver
—Ah... sí, obvio que sí. #speaker: roses
#cam: plano_medio_oliver
—Escúchenme, mañana tienen su primera presentación. Lleguen lo más sobrios posibles, ¿quieren? #speaker: oliver
-> a1_3_backstage


// -----------------------------------------------------
// 1.3 — INT. BACKSTAGE DEL ESTADIO — NOCHE
// -----------------------------------------------------
=== a1_3_backstage ===
#transition: disolver_humo
#hide: todos
#bg: backstage_detalle
#amb: estadio_rugido
#vfx: luces_estroboscopicas
#title: 1.3: Tras bambalinas
Desde un inicio con ellos, todo… fue caótico… #speaker: oliver_vo
—¡En cinco minutos entran, prepárense! #speaker: tecnico_os
—¿Aún no están aquí? Dios santo... #speaker: oliver
-> vestidor

= vestidor
#sfx: puerta_golpe
#bg: vestidor
#sprite: roses_maquillaje
#sprite: noah_maquillaje
#sprite: alex_chica
—¿Listos, chicos? #speaker: oliver
—¡Nope! Aún no... ¿Dónde está el idiota del bajo? #speaker: roses
—¿A quién le putas importa? En el baño, drogándose, supongo. #speaker: noah
—Alex, ve a buscar a Benny por lo que más quieras... Y tú, ponte algo de ropa, está tirada en la entrada. #speaker: oliver
#sprite: alex_grune
—¿Tenías que echarla así? ¿Sabes lo difícil que fue convencerla de que volviéramos a hablar? Imbécil. #speaker: alex
-> benny_dormido

= benny_dormido
#cam: enfoque_sillon
—Oigan, vean esto... #speaker: roses
#sfx: palmada_seca
#sprite: bennedict_despierta
—¡Ay! Mierda, ¿quién chingados me pegó? #speaker: bennedict
#sfx: risa_roses
—Hey, no es mi problema, el señor don mánager nos mandó a buscarte. #speaker: roses
#sprite: bennedict_agresivo
—¿Tenías que pegarme así de fuerte? #speaker: bennedict
-> discurso

= discurso
// DECISIÓN ILUSORIA: Oliver siempre termina gritando
* [Gritarles.]
* [Intentar calmarlos.]
* [Golpear la mesa.]
- —¡Basta, escúchenme todos de una vez! En menos de cinco minutos tienen que salir. Concéntrense por favor. Son espectaculares y la gente allá afuera lo sabe. #speaker: oliver
—Escuchen, terminando el show me iré directo a cerrar la renovación de los contratos con los ejecutivos de la discográfica. #speaker: oliver
—Ustedes tomarán la van con el chófer para viajar esta noche. Los veré mañana a mediodía en el hotel de la siguiente ciudad. ¿Entendido? #speaker: oliver
#sprite: roses_seguro
—Sí, sí, señor mánager. No te pongas taaaaaaan sentimental que me haces llorar. Lárgate, que tenemos un show que dar. #speaker: roses
—Háganme ganar un chingo de dinero entonces, bastardos imprudentes. #speaker: oliver
-> salida

= salida
@banda_sale #cam: pov_oliver #hide: todos #sfx: pasos_pasillo #wait: 2
@entran_a_la_luz #vfx: destello_blanco #sfx: estadio_gritos #wait: 2.5
-> fin_acto1


// -----------------------------------------------------
// TRANSICIÓN ACTO 1 → ACTO 2
// -----------------------------------------------------
=== fin_acto1 ===
@corte_negro #transition: corte_negro #sfx: golpe_tajante #wait: 3.5
@titulo_acto #act: ACTO 2 #sfx: golpe_tajante #wait: 2
-> a2_1_interrogantes
